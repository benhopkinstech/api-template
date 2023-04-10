using Api.Modules.Identity.Data;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Repositories;
using System.Net;
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
    }
}
