using Api.Modules.Identity.Data.Tables;

namespace Api.Modules.Identity.Interfaces
{
    public interface IEmailService
    {
        public Task<bool> SendVerificationLinkAsync(Verification verification, string recipient);
        public Task<bool> SendEmailChangedAsync(string recipient, string newEmail);
        public Task<bool> SendResetLinkAsync(Reset reset, string recipient);
    }
}
