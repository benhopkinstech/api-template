using Api.Modules.Identity.Data;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Api.Modules.Identity.Repositories;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace Api.Tests.Identity
{
    public class IdentityIntegrationTests : IntegrationTest
    {
        private IIdentityRepository _identity;
        private IdentityContext _dbContext;

        public IdentityIntegrationTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            _dbContext = CreateIdentityContext();
            _identity = new IdentityRepository(_dbContext);
        }

        [Fact]
        public async Task PostRegister()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, "", "");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PostLogin()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, "", "");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _configuration["Identity:VerificationRequired"] = "True";

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var account = await _identity.GetLocalAccountByEmailAsync(email);
            Assert.NotNull(account);
            await _identity.AmendAccountVerifiedAsync(account);
            await _identity.SaveChangesAsync();

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _configuration["Identity:VerificationRequired"] = "False";
        }

        [Fact]
        public async Task PutEmail()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var emailUpdate = new CredentialsModel();
            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var account = await _identity.GetLocalAccountByEmailAsync(email);
            Assert.NotNull(account);
            await _identity.AmendAccountVerifiedAsync(account);
            await _identity.SaveChangesAsync();
            Assert.True(account.Verified);

            emailUpdate.Email = email;
            emailUpdate.Password = password;
            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            emailUpdate.Email = "newemail@test.com";
            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            foreach (var entity in _dbContext.ChangeTracker.Entries().ToList())
                entity.Reload();
            Assert.Equal(account.Email, emailUpdate.Email);
            Assert.False(account.Verified);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, emailUpdate.Email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PutPassword()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var passwordUpdate = new PasswordUpdateModel();
            response = await _client.PutAsJsonAsync("identity/password", passwordUpdate);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.PutAsJsonAsync("identity/password", passwordUpdate);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            passwordUpdate.Password = password;
            passwordUpdate.CurrentPassword = password;
            response = await _client.PutAsJsonAsync("identity/password", passwordUpdate);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            passwordUpdate.Password = "newepassword";
            response = await _client.PutAsJsonAsync("identity/password", passwordUpdate);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, passwordUpdate.Password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostVerificationLinkAndGetVerify()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var account = await _identity.GetLocalAccountByEmailAsync(email);
            Assert.NotNull(account);
            account = await _identity.GetLocalAccountIncludeVerificationByIdAsync(account.Id);
            Assert.NotNull(account);
            var accountVerification = account.Verification;
            Assert.NotNull(accountVerification);
            Assert.Equal(1, accountVerification.Count);

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Emailing not setup for test
            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            account = await _identity.GetLocalAccountIncludeVerificationByIdAsync(account.Id);
            Assert.NotNull(account);
            accountVerification = account.Verification;
            Assert.NotNull(accountVerification);
            Assert.Equal(2, accountVerification.Count);

            var verificationList = accountVerification.ToList();
            var code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{verificationList[0].Id}&{verificationList[0].AccountId}&{verificationList[0].CreatedOn}"));
            await _client.GetAsync($"identity/verification?code={code}");
            foreach (var entity in _dbContext.ChangeTracker.Entries().ToList())
                entity.Reload();
            Assert.True(account.Verified);
            Assert.Equal(0, accountVerification.Count);
        }

        [Fact]
        public async Task PostResetLinkAndPutReset()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var account = await _identity.GetLocalAccountByEmailAsync(email);
            Assert.NotNull(account);
            account = await _identity.GetLocalAccountIncludePasswordResetByIdAsync(account.Id);
            Assert.NotNull(account);
            var accountPassword = account.Password;
            Assert.NotNull(accountPassword);
            var accountReset = account.Reset;
            Assert.NotNull(accountReset);
            Assert.Equal(0, accountReset.Count);

            var emailModel = new EmailModel();
            response = await _client.PostAsJsonAsync("identity/resetlink", emailModel);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Emailing not setup for test
            emailModel.Email = email;
            response = await _client.PostAsJsonAsync("identity/resetlink", emailModel);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            account = await _identity.GetLocalAccountIncludePasswordResetByIdAsync(account.Id);
            Assert.NotNull(account);
            accountPassword = account.Password;
            Assert.NotNull(accountPassword);
            accountReset = account.Reset;
            Assert.NotNull(accountReset);
            Assert.Equal(1, accountReset.Count);

            var resetList = accountReset.ToList();
            var code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{resetList[0].Id}&{resetList[0].AccountId}&{resetList[0].CreatedOn}"));
            var passwordModel = new PasswordModel();
            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", passwordModel);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            passwordModel.Password = "new password";
            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", passwordModel);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            foreach (var entity in _dbContext.ChangeTracker.Entries().ToList())
                entity.Reload();
            Assert.Equal(0, accountReset.Count);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, "new password");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.DeleteAsync("identity/delete");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
