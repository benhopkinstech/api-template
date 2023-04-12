using SendGrid.Helpers.Mail;
using SendGrid;
using Api.Modules.Identity.Data.Tables;
using System.Text;
using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _http;

        public EmailService(IConfiguration config, IHttpContextAccessor http)
        {
            _config = config;
            _http = http;
        }

        public async Task<bool> SendVerificationLinkAsync(Verification verification, string recipient)
        {
            if (ApiKeyUnpopulated())
                return false;

            var baseUrl = $"{_http.HttpContext?.Request.Scheme}://{_http.HttpContext?.Request.Host}/identity/verification?code=";
            var dynamicTemplateData = new
            {
                url = baseUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{verification.Id}&{verification.AccountId}&{verification.CreatedOn}")),
            };

            return await SendViaSendGridAsync(recipient, _config.GetValue<string>("SendGrid:VerificationLinkTemplateId") ?? "", dynamicTemplateData);
        }

        public async Task<bool> SendEmailChangedAsync(string recipient, string newEmail)
        {
            if (ApiKeyUnpopulated())
                return false;

            var dynamicTemplateData = new
            {
                newEmail,
            };

            return await SendViaSendGridAsync(recipient, _config.GetValue<string>("SendGrid:EmailChangedTemplateId") ?? "", dynamicTemplateData);
        }

        public async Task<bool> SendResetLinkAsync(Reset reset, string recipient)
        {
            if (ApiKeyUnpopulated())
                return false;

            var resetUrl = _config.GetValue<string>("Identity:ResetUrl");
            var dynamicTemplateData = new
            {
                url = resetUrl + Convert.ToBase64String(Encoding.Unicode.GetBytes($"{reset.Id}&{reset.AccountId}&{reset.CreatedOn}")),
            };

            return await SendViaSendGridAsync(recipient, _config.GetValue<string>("SendGrid:ResetLinkTemplateId") ?? "", dynamicTemplateData);
        }

        private async Task<bool> SendViaSendGridAsync(string recipient, string templateId, object dynamicTemplateData)
        {
            var client = new SendGridClient(_config.GetValue<string>("SendGrid:ApiKey"));
            var from = new EmailAddress(_config.GetValue<string>("SendGrid:Email"), _config.GetValue<string>("SendGrid:Name"));
            var to = new EmailAddress(recipient);
            SendGridMessage message = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(message);
            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }

        private bool ApiKeyUnpopulated()
        {
            string apiKey = _config.GetValue<string>("SendGrid:ApiKey") ?? "";

            if (String.IsNullOrWhiteSpace(apiKey) || apiKey.ToLower() == "disabled")
                return true;

            return false;
        }
    }
}
