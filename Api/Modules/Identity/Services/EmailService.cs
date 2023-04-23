using SendGrid.Helpers.Mail;
using SendGrid;
using Api.Modules.Identity.Data.Tables;
using System.Text;
using Api.Modules.Identity.Interfaces;
using Api.Settings;
using Microsoft.Extensions.Options;

namespace Api.Modules.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridSettings _settings;
        private readonly IHttpContextAccessor _http;

        public EmailService(IOptionsMonitor<SendGridSettings> settings, IHttpContextAccessor http)
        {
            _settings = settings.CurrentValue;
            _http = http;
        }

        public async Task<bool> SendVerificationLinkAsync(Verification verification, string recipient)
        {
            if (ApiKeyDisabled())
                return false;

            var baseUrl = $"{_http.HttpContext?.Request.Scheme}://{_http.HttpContext?.Request.Host}/identity/verification?code=";
            var dynamicTemplateData = new
            {
                url = baseUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{verification.Id}&{verification.AccountId}&{verification.CreatedOn}")),
            };

            return await SendViaSendGridAsync(recipient, _settings.VerificationLinkTemplateId, dynamicTemplateData);
        }

        public async Task<bool> SendEmailChangedAsync(string recipient, string newEmail)
        {
            if (ApiKeyDisabled())
                return false;

            var dynamicTemplateData = new
            {
                newEmail,
            };

            return await SendViaSendGridAsync(recipient, _settings.EmailChangedTemplateId, dynamicTemplateData);
        }

        public async Task<bool> SendResetLinkAsync(Reset reset, string recipient)
        {
            if (ApiKeyDisabled())
                return false;

            var resetUrl = _settings.ResetUrl;
            var dynamicTemplateData = new
            {
                url = resetUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{reset.Id}&{reset.AccountId}&{reset.CreatedOn}")),
            };

            return await SendViaSendGridAsync(recipient, _settings.ResetLinkTemplateId, dynamicTemplateData);
        }

        private async Task<bool> SendViaSendGridAsync(string recipient, string templateId, object dynamicTemplateData)
        {
            var client = new SendGridClient(_settings.ApiKey);
            var from = new EmailAddress(_settings.Email, _settings.Name);
            var to = new EmailAddress(recipient);
            SendGridMessage message = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(message);
            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }

        private bool ApiKeyDisabled ()
        {
            if (_settings.ApiKey.ToLower() == "disabled")
                return true;

            return false;
        }
    }
}
