using Api.Modules.Identity.Data.Tables;

namespace Api.Modules.Identity.Interfaces
{
    public interface IIdentityRepository
    {
        public Task<bool> AnyLocalAccountByEmailAsync(string email);
        public Task<Account?> GetAccountIncludeAllByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountAndPasswordByEmailAsync(string email);
        public Task<Account?> GetLocalAccountAndPasswordByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountAndVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountAndResetByEmailAsync(string email);
        public Task<Account?> GetLocalAccountPasswordAndVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountPasswordAndResetByIdAsync(Guid id);
        public Task<Verification?> GetVerificationByIdAsync(Guid id);
        public Task<Reset?> GetResetByIdAsync(Guid id);
        public Task<Account> AddAccountAsync(string email);
        public Task AddPasswordAsync(Guid accountId, string password);
        public Task<Verification> AddVerificationAsync(Guid accountId);
        public Task<Reset> AddResetAsync(Guid accountId);
        public Task<Account> AmendAccountEmailAsync(Account account, string email);
        public void AmendAccountVerified(Account account);
        public Task AmendPasswordAsync(Password passwordRecord, string password);
        public void RemoveRangeVerification(ICollection<Verification> verification);
        public void RemoveRangeReset(ICollection<Reset> reset);
        public Task InsertLoginAsync(Guid? accountId, string email, bool successful, HttpContext http);
        public Task DeleteAll(ICollection<PasswordAudit> passwordAudit, ICollection<AccountAudit> accountAudit, ICollection<Login> login, ICollection<Verification> verification,
            ICollection<Reset> reset, Password password, Account account);
        public Task SaveChangesAsync();
    }
}
