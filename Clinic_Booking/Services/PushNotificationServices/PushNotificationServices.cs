using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clinic_Booking.Configuration;
using Clinic_Booking.Data;
using Clinic_Booking.IServices.IPushNotificationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinic_Booking.Services.PushNotificationServices
{
    public class PushNotificationServices : IPushNotificationServices
    {
        private const string FirebaseScope = "https://www.googleapis.com/auth/firebase.messaging";
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly PushNotificationOptions _options;
        private readonly ILogger<PushNotificationServices> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _cachedAccessToken;
        private DateTimeOffset _cachedAccessTokenExpiresAt;
        private FirebaseServiceAccount? _serviceAccount;

        public PushNotificationServices(
            ApplicationDbContext context,
            HttpClient httpClient,
            IOptions<PushNotificationOptions> options,
            ILogger<PushNotificationServices> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendToUserAsync(
            Guid userId,
            string title,
            string body,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Push send requested. UserId={UserId}, Title={Title}, DataKeys={DataKeys}",
                userId,
                title,
                data == null ? "" : string.Join(",", data.Keys));

            if (!_options.Enabled)
            {
                _logger.LogInformation("Push notification skipped for user {UserId}: provider disabled.", userId);
                return false;
            }

            var tokens = await _context.DeviceTokens
                .Where(token => token.UserId == userId && !token.IsDeleted)
                .Select(token => token.Token)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0)
            {
                var tokenStats = await _context.DeviceTokens
                    .Where(token => token.UserId == userId)
                    .GroupBy(token => 1)
                    .Select(group => new
                    {
                        Total = group.Count(),
                        Active = group.Count(token => !token.IsDeleted),
                        Deleted = group.Count(token => token.IsDeleted)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                _logger.LogInformation(
                    "Push notification skipped for user {UserId}: no active device tokens. Total={Total}, Active={Active}, Deleted={Deleted}",
                    userId,
                    tokenStats?.Total ?? 0,
                    tokenStats?.Active ?? 0,
                    tokenStats?.Deleted ?? 0);
                return false;
            }

            _logger.LogInformation(
                "Push notification will be sent to {TokenCount} token(s) for user {UserId}.",
                tokens.Count,
                userId);

            var sentCount = 0;
            foreach (var token in tokens)
            {
                if (await SendFirebaseAsync(token, title, body, data, cancellationToken))
                {
                    sentCount++;
                }
            }

            return sentCount > 0;
        }

        private async Task<bool> SendFirebaseAsync(
            string token,
            string title,
            string body,
            IDictionary<string, string>? data,
            CancellationToken cancellationToken)
        {
            var serviceAccount = await GetServiceAccountAsync(cancellationToken);
            if (serviceAccount == null)
            {
                _logger.LogWarning("Firebase push skipped for token {Token}: service account unavailable.", MaskToken(token));
                return false;
            }

            var projectId = _options.ProjectId?.Trim();
            if (string.IsNullOrWhiteSpace(projectId))
            {
                projectId = serviceAccount.ProjectId;
            }

            if (string.IsNullOrWhiteSpace(projectId))
            {
                _logger.LogWarning("Firebase push skipped: project id is missing.");
                return false;
            }

            var accessToken = await GetAccessTokenAsync(serviceAccount, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Firebase push skipped for token {Token}: access token unavailable.", MaskToken(token));
                return false;
            }

            var endpoint = _options.FcmEndpoint.Replace("{projectId}", projectId);
            _logger.LogInformation(
                "Sending Firebase push. ProjectId={ProjectId}, Token={Token}, Endpoint={Endpoint}",
                projectId,
                MaskToken(token),
                endpoint);

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(new
            {
                message = new
                {
                    token,
                    notification = new { title, body },
                    data = data ?? new Dictionary<string, string>()
                }
            });

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Firebase push failed. Token={Token}, Status={StatusCode}, Response={ResponseBody}",
                        MaskToken(token),
                        response.StatusCode,
                        responseBody);
                    return false;
                }

                _logger.LogInformation(
                    "Firebase push sent successfully. Token={Token}, Status={StatusCode}, Response={ResponseBody}",
                    MaskToken(token),
                    response.StatusCode,
                    responseBody);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase push failed with exception. Token={Token}", MaskToken(token));
                return false;
            }
        }

        private async Task<FirebaseServiceAccount?> GetServiceAccountAsync(CancellationToken cancellationToken)
        {
            if (_serviceAccount != null)
            {
                return _serviceAccount;
            }

            var serviceAccountPath = _options.ServiceAccountPath?.Trim();
            _logger.LogInformation("Loading Firebase service account from {Path}.", serviceAccountPath);
            if (string.IsNullOrWhiteSpace(serviceAccountPath))
            {
                _logger.LogWarning("Firebase push skipped: service account path is missing.");
                return null;
            }

            if (!File.Exists(serviceAccountPath))
            {
                _logger.LogWarning("Firebase push skipped: service account file was not found at {Path}.", serviceAccountPath);
                return null;
            }

            await using var stream = File.OpenRead(serviceAccountPath);
            _serviceAccount = await JsonSerializer.DeserializeAsync<FirebaseServiceAccount>(
                stream,
                cancellationToken: cancellationToken);

            if (_serviceAccount == null ||
                string.IsNullOrWhiteSpace(_serviceAccount.ClientEmail) ||
                string.IsNullOrWhiteSpace(_serviceAccount.PrivateKey) ||
                string.IsNullOrWhiteSpace(_serviceAccount.TokenUri))
            {
                _logger.LogWarning("Firebase push skipped: service account file is incomplete.");
                return null;
            }

            _logger.LogInformation(
                "Firebase service account loaded. ProjectId={ProjectId}, ClientEmail={ClientEmail}, TokenUri={TokenUri}",
                _serviceAccount.ProjectId,
                _serviceAccount.ClientEmail,
                _serviceAccount.TokenUri);

            return _serviceAccount;
        }

        private async Task<string?> GetAccessTokenAsync(
            FirebaseServiceAccount serviceAccount,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_cachedAccessToken) &&
                _cachedAccessTokenExpiresAt > BusinessClock.NowOffset().AddMinutes(2))
            {
                return _cachedAccessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!string.IsNullOrWhiteSpace(_cachedAccessToken) &&
                    _cachedAccessTokenExpiresAt > BusinessClock.NowOffset().AddMinutes(2))
                {
                    _logger.LogDebug("Using cached Firebase access token. ExpiresAt={ExpiresAt}", _cachedAccessTokenExpiresAt);
                    return _cachedAccessToken;
                }

                _logger.LogInformation("Requesting Firebase access token. ClientEmail={ClientEmail}", serviceAccount.ClientEmail);
                var assertion = CreateJwtAssertion(serviceAccount);
                using var response = await _httpClient.PostAsync(
                    serviceAccount.TokenUri,
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                        ["assertion"] = assertion
                    }),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Firebase access token request failed with status {StatusCode}. Response: {ResponseBody}",
                        response.StatusCode,
                        responseBody);
                    return null;
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<FirebaseTokenResponse>(
                    cancellationToken: cancellationToken);

                if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                {
                    _logger.LogWarning("Firebase access token response did not include an access token.");
                    return null;
                }

                _cachedAccessToken = tokenResponse.AccessToken;
                _cachedAccessTokenExpiresAt = BusinessClock.NowOffset().AddSeconds(tokenResponse.ExpiresIn);
                _logger.LogInformation("Firebase access token acquired. ExpiresAt={ExpiresAt}", _cachedAccessTokenExpiresAt);
                return _cachedAccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase access token request failed.");
                return null;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private static string MaskToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "(empty)";
            }

            if (token.Length <= 16)
            {
                return $"{token[..Math.Min(4, token.Length)]}...";
            }

            return $"{token[..8]}...{token[^6..]}";
        }

        private static string CreateJwtAssertion(FirebaseServiceAccount serviceAccount)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(serviceAccount.PrivateKey);

            var now = BusinessClock.NowOffset();
            var payload = new JwtPayload
            {
                { "iss", serviceAccount.ClientEmail },
                { "scope", FirebaseScope },
                { "aud", serviceAccount.TokenUri },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(55).ToUnixTimeSeconds() }
            };

            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(true)),
                SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(new JwtHeader(credentials), payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private sealed class FirebaseServiceAccount
        {
            [JsonPropertyName("project_id")]
            public string? ProjectId { get; set; }

            [JsonPropertyName("private_key")]
            public string? PrivateKey { get; set; }

            [JsonPropertyName("client_email")]
            public string? ClientEmail { get; set; }

            [JsonPropertyName("token_uri")]
            public string? TokenUri { get; set; }
        }

        private sealed class FirebaseTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
