using Api.Modules.Identity.Data.Tables;

namespace Api.Modules.Identity.Interfaces
{
    public interface IAuthService
    {
        public string GenerateTokens(Guid accountId, string email, Refresh refresh);
        public Guid? GetAccountId();
    }
}
