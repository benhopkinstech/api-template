﻿using Api.Modules.Identity.Data.Tables;

namespace Api.Modules.Identity.Interfaces
{
    public interface IIdentityRepository
    {
        public Task<bool> AnyLocalAccountByEmailAsync(string email);
        public Task<Account?> GetAccountIncludeAllByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountByEmailAsync(string email);
        public Task<Account?> GetLocalAccountIncludePasswordByEmailAsync(string email);
        public Task<Account?> GetLocalAccountIncludeVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountIncludePasswordVerificationByIdAsync(Guid id);
        public Task<Account?> GetLocalAccountIncludePasswordResetByIdAsync(Guid id);
        public Task<Password?> GetPasswordByAccountIdAsync(Guid accountId);
        public Task<Verification?> GetVerificationByIdAsync(Guid id);
        public Task<Reset?> GetResetByIdAsync(Guid id);
        public Task<Account> AddLocalAccountAsync(string email);
        public Task AddPasswordAsync(Guid accountId, string password);
        public Task<Verification> AddVerificationAsync(Guid accountId);
        public Task<Reset> AddResetAsync(Guid accountId);
        public Task<Account> AmendAccountEmailAsync(Account account, string email);
        public Task AmendAccountVerified(Account account);
        public Task AmendPasswordAsync(Password passwordRecord, string password);
        public Task RemoveRangeVerification(ICollection<Verification> verification);
        public Task RemoveRangeReset(ICollection<Reset> reset);
        public Task InsertLoginAsync(Guid? accountId, string email, bool successful, HttpContext http);
        public Task DeleteAll(ICollection<PasswordAudit> passwordAudit, ICollection<AccountAudit> accountAudit, ICollection<Login> login, ICollection<Verification> verification,
            ICollection<Reset> reset, Password password, Account account);
        public Task SaveChangesAsync();
    }
}
