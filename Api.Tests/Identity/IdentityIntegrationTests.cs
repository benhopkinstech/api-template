using Api.Modules.Identity.Data;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Api.Modules.Identity.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace Api.Tests.Identity
{
    public class IdentityIntegrationTests : IntegrationTest
    {
        private IIdentityService _identity;
        private IdentityContext _dbContext;

        public IdentityIntegrationTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            _dbContext = CreateIdentityContext();
            _identity = new IdentityService(_dbContext, _http);
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
            Assert.True(account.IsVerified);

            emailUpdate.Email = email;
            emailUpdate.Password = password;
            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            emailUpdate.Email = "newemail@test.com";
            response = await _client.PutAsJsonAsync("identity/email", emailUpdate);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            foreach (var entity in _dbContext.ChangeTracker.Entries().ToList())
                entity.Reload();
            Assert.Equal(account.Email, emailUpdate.Email);
            Assert.False(account.IsVerified);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

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

            var update = new PasswordUpdateModel();
            response = await _client.PutAsJsonAsync("identity/password", update);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.PutAsJsonAsync("identity/password", update);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            update.Password = "newpassword";
            update.CurrentPassword = "newpassword";
            response = await _client.PutAsJsonAsync("identity/password", update);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            update.CurrentPassword = "currentpassword";
            response = await _client.PutAsJsonAsync("identity/password", update);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            update.CurrentPassword = password;
            response = await _client.PutAsJsonAsync("identity/password", update);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, update.Password);
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
            Guid.TryParse(response.Headers.Location?.OriginalString, out Guid accountId);
            var account = await _identity.GetLocalAccountIncludeVerificationByIdAsync(accountId);
            Assert.NotNull(account);
            var accountVerification = account.Verification;
            Assert.NotNull(accountVerification);
            Assert.Equal(1, accountVerification.Count);

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);
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
            Assert.True(account.IsVerified);
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
            Guid.TryParse(response.Headers.Location?.OriginalString, out Guid accountId);
            var account = await _identity.GetLocalAccountIncludePasswordResetByIdAsync(accountId);
            Assert.NotNull(account);
            var accountPassword = account.Password;
            Assert.NotNull(accountPassword);
            var accountReset = account.Reset;
            Assert.NotNull(accountReset);
            Assert.Equal(0, accountReset.Count);

            var resetLink = new EmailModel();
            response = await _client.PostAsJsonAsync("identity/resetlink", resetLink);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            resetLink.Email = email;
            response = await _client.PostAsJsonAsync("identity/resetlink", resetLink);
            Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);
            account = await _identity.GetLocalAccountIncludePasswordResetByIdAsync(account.Id);
            Assert.NotNull(account);
            accountPassword = account.Password;
            Assert.NotNull(accountPassword);
            accountReset = account.Reset;
            Assert.NotNull(accountReset);
            Assert.Equal(1, accountReset.Count);

            var resetList = accountReset.ToList();
            var code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{resetList[0].Id}&{resetList[0].AccountId}&{resetList[0].CreatedOn}"));
            var reset = new PasswordModel();
            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", reset);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            reset.Password = "new password";
            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", reset);
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
        public async Task PutDelete()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var delete = new PasswordModel();
            response = await _client.PutAsJsonAsync("identity/delete", delete);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.PutAsJsonAsync("identity/delete", delete);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            delete.Password = "somepassword";
            response = await _client.PutAsJsonAsync("identity/delete", delete);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            delete.Password = password;
            response = await _client.PutAsJsonAsync("identity/delete", delete);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _client.PutAsJsonAsync("identity/delete", delete);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
