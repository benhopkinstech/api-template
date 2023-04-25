using SendGrid.Helpers.Mail;
using SendGrid;
using Api.Modules.Identity.Data.Tables;
using System.Text;
using Api.Modules.Identity.Interfaces;
using Api.Options;

namespace Api.Modules.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridOptions _options;
        private readonly IHttpContextAccessor _http;

        public EmailService(SendGridOptions options, IHttpContextAccessor http)
        {
            _options = options;
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

            return await SendViaSendGridAsync(recipient, _options.VerificationLinkTemplateId, dynamicTemplateData);
        }

        public async Task<bool> SendEmailChangedAsync(string recipient, string newEmail)
        {
            if (ApiKeyDisabled())
                return false;

            var dynamicTemplateData = new
            {
                newEmail,
            };

            return await SendViaSendGridAsync(recipient, _options.EmailChangedTemplateId, dynamicTemplateData);
        }

        public async Task<bool> SendResetLinkAsync(Reset reset, string recipient)
        {
            if (ApiKeyDisabled())
                return false;

            var resetUrl = _options.ResetUrl;
            var dynamicTemplateData = new
            {
                url = resetUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{reset.Id}&{reset.AccountId}&{reset.CreatedOn}")),
            };

            return await SendViaSendGridAsync(recipient, _options.ResetLinkTemplateId, dynamicTemplateData);
        }

        private async Task<bool> SendViaSendGridAsync(string recipient, string templateId, object dynamicTemplateData)
        {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.Email, _options.Name);
            var to = new EmailAddress(recipient);
            SendGridMessage message = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(message);
            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }

        private bool ApiKeyDisabled ()
        {
            if (_options.ApiKey.ToLower() == "disabled")
                return true;

            return false;
        }
    }
}
