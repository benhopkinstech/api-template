﻿using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Repositories
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly IdentityContext _identity;

        public IdentityRepository(IdentityContext identity)
        {
            _identity = identity;
        }

        public async Task<bool> AnyLocalAccountByEmailAsync(string email)
        {
            return await _identity.Account.AnyAsync(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == email);
        }

        public async Task<Account?> GetAccountIncludeAllByIdAsync(Guid id)
        {
            return await _identity.Account.Where(x => x.Id == id).Include(x => x.PasswordAudit).Include(x => x.AccountAudit).Include(x => x.Login)
                .Include(x => x.Verification).Include(x => x.Reset).Include(x => x.Password).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetLocalAccountIncludePasswordByEmailAsync(string email)
        {
            return await _identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == email).Include(x => x.Password).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetLocalAccountIncludeVerificationByIdAsync(Guid id)
        {
            return await _identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == id).Include(x => x.Verification).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetLocalAccountIncludeResetByEmailAsync(string email)
        {
            return await _identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == email).Include(x => x.Reset).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetLocalAccountIncludePasswordVerificationByIdAsync(Guid id)
        {
            return await _identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == id).Include(x => x.Password).Include(x => x.Verification).FirstOrDefaultAsync();
        }

        public async Task<Account?> GetLocalAccountIncludePasswordResetByIdAsync(Guid id)
        {
            return await _identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == id).Include(x => x.Password).Include(x => x.Reset).FirstOrDefaultAsync();
        }

        public async Task<Password?> GetPasswordByAccountIdAsync(Guid accountId)
        {
            return await _identity.Password.Where(x => x.AccountId == accountId).FirstOrDefaultAsync();
        }

        public async Task<Verification?> GetVerificationByIdAsync(Guid id)
        {
            return await _identity.Verification.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Reset?> GetResetByIdAsync(Guid id)
        {
            return await _identity.Reset.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Account> AddLocalAccountAsync(string email)
        {
            var account = new Account { Id = Guid.NewGuid(), ProviderId = (short)Enums.Provider.Local, Email = email };
            await _identity.Account.AddAsync(account);
            return account;
        }

        public async Task AddPasswordAsync(Guid accountId, string password)
        {
            await _identity.Password.AddAsync(new Password { AccountId = accountId, Hash = Encryption.GenerateHash(password) });
        }

        public async Task<Verification> AddVerificationAsync(Guid accountId)
        {
            var verification = new Verification { Id = Guid.NewGuid(), AccountId = accountId, CreatedOn = DateTime.UtcNow };
            await _identity.Verification.AddAsync(verification);
            return verification;
        }

        public async Task<Reset> AddResetAsync(Guid accountId)
        {
            var reset = new Reset { Id = Guid.NewGuid(), AccountId = accountId, CreatedOn = DateTime.UtcNow };
            await _identity.Reset.AddAsync(reset);
            return reset;
        }

        public async Task<Account> AmendAccountEmailAsync(Account account, string email)
        {
            await _identity.AccountAudit.AddAsync(new AccountAudit { AccountId = account.Id, Email = account.Email });
            account.Email = email;
            account.Verified = false;
            account.VerifiedOn = null;
            account.UpdatedOn = DateTime.UtcNow;
            return account;
        }

        public void AmendAccountVerified(Account account)
        {
            account.Verified = true;
            account.VerifiedOn = DateTime.UtcNow;
            account.UpdatedOn = DateTime.UtcNow;
        }

        public async Task AmendPasswordAsync(Password passwordRecord, string password)
        {
            await _identity.PasswordAudit.AddAsync(new PasswordAudit { AccountId = passwordRecord.AccountId, Hash = passwordRecord.Hash });
            passwordRecord.Hash = Encryption.GenerateHash(password);
            passwordRecord.UpdatedOn = DateTime.UtcNow;
        }

        public void RemoveRangeVerification(ICollection<Verification> verification)
        {
            _identity.Verification.RemoveRange(verification);
        }

        public void RemoveRangeReset(ICollection<Reset> reset)
        {
            _identity.Reset.RemoveRange(reset);
        }

        public async Task InsertLoginAsync(Guid? accountId, string email, bool successful, HttpContext http)
        {
            await _identity.Login.AddAsync(new Login { AccountId = accountId, Email = email, Successful = successful, IpAddress = http.Connection.RemoteIpAddress });
            await _identity.SaveChangesAsync();
        }

        public async Task DeleteAll(ICollection<PasswordAudit> passwordAudit, ICollection<AccountAudit> accountAudit, ICollection<Login> login, ICollection<Verification> verification, ICollection<Reset> reset, Password password, Account account)
        {
            _identity.PasswordAudit.RemoveRange(passwordAudit);
            _identity.AccountAudit.RemoveRange(accountAudit);
            _identity.Login.RemoveRange(login);
            _identity.Verification.RemoveRange(verification);
            _identity.Reset.RemoveRange(reset);
            _identity.Password.Remove(password);
            _identity.Account.Remove(account);
            await _identity.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _identity.SaveChangesAsync();
        }
    }
}
