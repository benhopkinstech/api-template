using Api.Modules.Identity.Data.Tables;

namespace Api.Modules.Identity.Interfaces
{
    public interface IIdentityService
    {
        public Task<bool> AnyLocalAccountByEmailAsync(string email);
        public Task<Account?> GetAccountIncludeAllByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountByEmailAsync(string email);
        public Task<Account?> GetLocalAccountIncludePasswordByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountIncludePasswordByEmailAsync(string email);
        public Task<Account?> GetLocalAccountIncludeVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountIncludeResetByEmailAsync(string email);
        public Task<Account?> GetLocalAccountIncludePasswordVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountIncludePasswordResetByIdAsync(Guid id);
        public Task<Verification?> GetVerificationByIdAsync(Guid id);
        public Task<Reset?> GetResetByIdAsync(Guid id);
        public Task<Refresh?> GetRefreshIncludeAccountByIdAsync(Guid id);
        public Task<ICollection<Refresh>> GetRefreshListNotUsedByAccountIdAsync(Guid id);
        public Task<Account> AddLocalAccountAsync(string email);
        public Task AddPasswordAsync(Guid accountId, string password);
        public Task<Verification> AddVerificationAsync(Guid accountId);
        public Task<Reset> AddResetAsync(Guid accountId);
        public Task<Refresh> AddRefreshAsync(Guid accountId, DateTime expiry);
        public Task AddLoginAsync(Guid? accountId, string email, bool successful);
        public Task<Account> AmendAccountEmailAsync(Account account, string email);
        public Task AmendAccountVerifiedAsync(Account account);
        public Task AmendPasswordAsync(Password passwordRecord, string password);
        public Task AmendRefreshUsedAsync(Refresh refresh);
        public Task RemoveVerificationAsync(Verification verification);
        public Task RemoveResetAsync(Reset reset);
        public Task RemoveAll(ICollection<PasswordAudit> passwordAudit, ICollection<AccountAudit> accountAudit, ICollection<Login> login, ICollection<Refresh> refresh, 
            Verification? verification, Reset? reset, Password password, Account account);
        public Task SaveChangesAsync();
    }
}
