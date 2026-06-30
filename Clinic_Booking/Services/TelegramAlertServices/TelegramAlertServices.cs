using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clinic_Booking.Configuration;
using Clinic_Booking.IServices.ITelegramAlertService;
using Microsoft.Extensions.Options;

namespace Clinic_Booking.Services.TelegramAlertServices
{
    public class TelegramAlertService : ITelegramAlertService
    {
        private const string TelegramApiBase = "https://api.telegram.org/bot{0}/sendMessage";
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RateLimitDelay = TimeSpan.FromMilliseconds(350);

        private readonly TelegramAlertOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TelegramAlertService> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _recentAlerts = new();

        public TelegramAlertService(
            IOptions<TelegramAlertOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<TelegramAlertService> logger)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task SendOtpFailureAlertAsync(
            string phoneNumber,
            string otpCode,
            string messageText,
            string errorReason)
        {
            var text = new StringBuilder();
            text.AppendLine("\U0001f6a8 WhatsApp OTP Failed");
            text.AppendLine();
            AppendProjectInfo(text);
            text.AppendLine();
            AppendField(text, "Phone", phoneNumber);
            AppendField(text, "OTP", otpCode);
            AppendField(text, "Message", messageText);
            text.AppendLine();
            AppendField(text, "Reason", errorReason);
            text.AppendLine();
            AppendTime(text);

            return SendSafeAsync(text.ToString());
        }

        public Task SendNewDoctorRequestAlertAsync(
            string fullName,
            string phoneNumber,
            string province,
            string? knownName)
        {
            var text = new StringBuilder();
            text.AppendLine("\U0001fa97 New Doctor Request");
            text.AppendLine();
            AppendProjectInfo(text);
            text.AppendLine();
            AppendField(text, "Name", fullName);
            AppendField(text, "Phone", phoneNumber);
            AppendField(text, "Province", province);
            if (!string.IsNullOrWhiteSpace(knownName))
            {
                AppendField(text, "Known Name", knownName);
            }
            text.AppendLine();
            text.AppendLine("Status: Pending Review");
            AppendTime(text);

            return SendSafeAsync(text.ToString());
        }

        public Task SendBackendExceptionAlertAsync(
            string? requestPath,
            string? httpMethod,
            string? userId,
            string exceptionMessage)
        {
            var dedupKey = "BackendEx_" + (exceptionMessage.Length > 100 ? exceptionMessage[..100] : exceptionMessage);
            if (!ShouldSend(dedupKey))
            {
                return Task.CompletedTask;
            }

            var text = new StringBuilder();
            text.AppendLine("\U0001f525 Backend Exception");
            text.AppendLine();
            AppendProjectInfo(text);
            text.AppendLine();
            if (requestPath != null) AppendField(text, "Path", requestPath);
            if (httpMethod != null) AppendField(text, "Method", httpMethod);
            if (userId != null) AppendField(text, "UserId", userId);
            text.AppendLine();
            AppendField(text, "Error", exceptionMessage);
            text.AppendLine();
            AppendTime(text);

            return SendSafeAsync(text.ToString(), dedupKey);
        }

        public Task SendPushNotificationFailureAlertAsync(
            Guid userId,
            string title,
            string errorReason)
        {
            var text = new StringBuilder();
            text.AppendLine("\U0001f514 Push Notification Failed");
            text.AppendLine();
            AppendProjectInfo(text);
            text.AppendLine();
            AppendField(text, "UserId", userId.ToString());
            AppendField(text, "Title", title);
            AppendField(text, "Reason", errorReason);
            text.AppendLine();
            AppendTime(text);

            return SendSafeAsync(text.ToString(), "PushFail_" + userId);
        }

        public Task SendWhatsAppBridgeFailureAlertAsync(
            string? phoneNumber,
            string action,
            string errorReason)
        {
            var dedupKey = "WhatsAppBridge_" + (phoneNumber ?? "unknown") + "_" + action;
            if (!ShouldSend(dedupKey))
            {
                return Task.CompletedTask;
            }

            var text = new StringBuilder();
            text.AppendLine("\U0001f4f5 WhatsApp Bridge Failure");
            text.AppendLine();
            AppendProjectInfo(text);
            text.AppendLine();
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                AppendField(text, "Phone", phoneNumber);
            }
            AppendField(text, "Action", action);
            AppendField(text, "Reason", errorReason);
            text.AppendLine();
            AppendTime(text);

            return SendSafeAsync(text.ToString(), dedupKey);
        }

        private async Task SendSafeAsync(string messageText, string? dedupKey = null)
        {
            if (!_options.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.BotToken) || string.IsNullOrWhiteSpace(_options.ChatId))
            {
                _logger.LogWarning("Telegram alert skipped: BotToken or ChatId is not configured.");
                return;
            }

            if (!ShouldSend(dedupKey))
            {
                _logger.LogDebug("Telegram alert skipped: duplicate suppressed for key {DedupKey}.", dedupKey);
                return;
            }

            try
            {
                using var client = _httpClientFactory.CreateClient("TelegramAlert");
                var url = string.Format(TelegramApiBase, _options.BotToken);
                var payload = new TelegramSendMessageRequest
                {
                    ChatId = _options.ChatId,
                    Text = messageText,
                    ParseMode = "HTML",
                    DisableWebPagePreview = true
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json")
                };

                using var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Telegram send failed. Status={StatusCode}, Response={ResponseBody}",
                        response.StatusCode,
                        responseBody);
                }

                await Task.Delay(RateLimitDelay);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Telegram alert send failed for key {DedupKey}.", dedupKey);
            }
        }

        private bool ShouldSend(string? dedupKey)
        {
            if (string.IsNullOrEmpty(dedupKey))
            {
                return true;
            }

            if (_recentAlerts.TryGetValue(dedupKey, out var lastSent))
            {
                if (BusinessClock.Now() - lastSent < DuplicateWindow)
                {
                    return false;
                }
            }

            _recentAlerts[dedupKey] = BusinessClock.Now();
            CleanupStaleEntries();
            return true;
        }

        private void CleanupStaleEntries()
        {
            if (_recentAlerts.Count <= 1000)
            {
                return;
            }

            var cutoff = BusinessClock.Now().AddMinutes(-5);
            foreach (var kvp in _recentAlerts)
            {
                if (kvp.Value < cutoff)
                {
                    _recentAlerts.TryRemove(kvp.Key, out _);
                }
            }
        }

        private void AppendProjectInfo(StringBuilder sb)
        {
            sb.AppendLine($"Project: {HtmlEscape(_options.ApplicationName)}");
            sb.AppendLine($"Environment: {HtmlEscape(_options.EnvironmentName)}");
        }

        private static void AppendField(StringBuilder sb, string label, string value)
        {
            sb.AppendLine($"<b>{HtmlEscape(label)}:</b> {HtmlEscape(value)}");
        }

        private static void AppendTime(StringBuilder sb)
        {
            sb.Append($"Time: {BusinessClock.Now():yyyy-MM-dd HH:mm:ss}");
        }

        internal static string HtmlEscape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private sealed class TelegramSendMessageRequest
        {
            [JsonPropertyName("chat_id")]
            public string ChatId { get; set; } = string.Empty;

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("parse_mode")]
            public string ParseMode { get; set; } = "HTML";

            [JsonPropertyName("disable_web_page_preview")]
            public bool DisableWebPagePreview { get; set; }
        }
    }
}
