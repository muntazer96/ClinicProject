using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clinic_Booking.Configuration;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Microsoft.Extensions.Options;

namespace Clinic_Booking.Services.WhatsAppMessageServices
{
    public class WhatsAppMessageServices : IWhatsAppMessageServices
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppMessageOptions _options;
        private readonly ILogger<WhatsAppMessageServices> _logger;

        public WhatsAppMessageServices(
            HttpClient httpClient,
            IOptions<WhatsAppMessageOptions> options,
            ILogger<WhatsAppMessageServices> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("WhatsApp message skipped for {PhoneNumber}: provider disabled.", phoneNumber);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.Endpoint))
            {
                _logger.LogWarning("WhatsApp message skipped for {PhoneNumber}: endpoint is missing.", phoneNumber);
                return false;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
            if (!string.IsNullOrWhiteSpace(_options.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
            }

            request.Content = JsonContent.Create(new Dictionary<string, string>
            {
                [_options.PhoneFieldName] = NormalizePhone(phoneNumber),
                [_options.MessageFieldName] = message
            });

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp message sent to {PhoneNumber}.", phoneNumber);
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "WhatsApp message failed for {PhoneNumber}. Status={StatusCode}, Response={ResponseBody}",
                    phoneNumber,
                    response.StatusCode,
                    responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp message failed for {PhoneNumber}.", phoneNumber);
                return false;
            }
        }

        private static string NormalizePhone(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0")) return $"964{digits[1..]}";
            return digits;
        }
    }
}
