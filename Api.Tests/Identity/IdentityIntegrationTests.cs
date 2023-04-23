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

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password + " ");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostRefresh()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await _client.PostAsync("identity/refresh", null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Guid.TryParse(response.Headers.Location?.OriginalString, out Guid accountId);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _client.DefaultRequestHeaders.Add("X-Refresh-Token", "test");
            response = await _client.PostAsync("identity/refresh", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var activeRefreshTokens = await _identity.GetRefreshListNotExpiredNotUsedByAccountIdAsync(accountId);
            Assert.Single(activeRefreshTokens);
            var refreshToken = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{activeRefreshTokens[0].Id}&{activeRefreshTokens[0].Secret}"));
            _client.DefaultRequestHeaders.Remove("X-Refresh-Token");
            _client.DefaultRequestHeaders.Add("X-Refresh-Token", refreshToken);
            response = await _client.PostAsync("identity/refresh", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            activeRefreshTokens = await _identity.GetRefreshListNotExpiredNotUsedByAccountIdAsync(accountId);
            Assert.Single(activeRefreshTokens);
            var newRefreshToken = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{activeRefreshTokens[0].Id}&{activeRefreshTokens[0].Secret}"));
            Assert.NotEqual(refreshToken, newRefreshToken);
            _client.DefaultRequestHeaders.Remove("X-Refresh-Token");
            _client.DefaultRequestHeaders.Add("X-Refresh-Token", newRefreshToken);
            response = await _client.PostAsync("identity/refresh", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            activeRefreshTokens = await _identity.GetRefreshListNotExpiredNotUsedByAccountIdAsync(accountId);
            Assert.Single(activeRefreshTokens);

            _client.DefaultRequestHeaders.Remove("X-Refresh-Token");
            _client.DefaultRequestHeaders.Add("X-Refresh-Token", refreshToken);
            response = await _client.PostAsync("identity/refresh", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            activeRefreshTokens = await _identity.GetRefreshListNotExpiredNotUsedByAccountIdAsync(accountId);
            Assert.Empty(activeRefreshTokens);
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

            await _dbContext.Entry(account).ReloadAsync();
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

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            response = await IdentityExtensions.LoginWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{accountVerification.Id}&{accountVerification.AccountId}&{accountVerification.CreatedOn}"));
            await _client.GetAsync($"identity/verification?code={code}");
            await _dbContext.Entry(account).ReloadAsync();
            Assert.True(account.IsVerified);
            var oldVerification = await _identity.GetVerificationByIdAsync(accountVerification.Id);
            Assert.Null(oldVerification);

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            account.IsVerified = false;
            await _identity.SaveChangesAsync();

            response = await _client.PostAsync("identity/verificationlink", null);
            Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);
        }

        [Fact]
        public async Task PostResetLinkAndPutReset()
        {
            await ResetDatabse();
            string email = "test@test.com";
            string password = "password";

            var response = await IdentityExtensions.RegisterWithCredentialsAsync(_client, email, password);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var account = await _identity.GetLocalAccountIncludeResetByEmailAsync(email);
            Assert.NotNull(account);
            var accountReset = account.Reset;
            Assert.Null(accountReset);

            var resetLink = new EmailModel();
            response = await _client.PostAsJsonAsync("identity/resetlink", resetLink);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            resetLink.Email = email;
            response = await _client.PostAsJsonAsync("identity/resetlink", resetLink);
            Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);
            account = await _identity.GetLocalAccountIncludeResetByEmailAsync(account.Email);
            Assert.NotNull(account);
            accountReset = account.Reset;
            Assert.NotNull(accountReset);

            var code = Convert.ToBase64String(Encoding.Unicode.GetBytes($"{accountReset.Id}&{accountReset.AccountId}&{accountReset.CreatedOn}"));
            var reset = new PasswordModel();
            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", reset);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            reset.Password = "new password";
            response = await _client.PutAsJsonAsync($"identity/reset?code=test", reset);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            response = await _client.PutAsJsonAsync($"identity/reset?code={code}", reset);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var oldReset = await _identity.GetResetByIdAsync(accountReset.Id);
            Assert.Null(oldReset);

            response = await _client.PostAsJsonAsync("identity/resetlink", resetLink);
            Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);

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

        [Fact]
        public async Task RateLimiting()
        {
            await ResetDatabse();
            var credentials = new CredentialsModel();
            var passwordUpdate = new PasswordUpdateModel();
            var email = new EmailModel();
            var password = new PasswordModel();

            var ipAddresses = new[] { "1.2.3.4", "4.3.2.1" };

            var unauthorizedEndpoints = new Dictionary<Func<Task<HttpResponseMessage>>, (int Limit, HttpStatusCode Status)>
            {
                { () => _client.PostAsJsonAsync("identity/register", credentials), (10, HttpStatusCode.BadRequest) },
                { () => _client.PostAsJsonAsync("identity/login", credentials), (10, HttpStatusCode.BadRequest) },
                { () => _client.PostAsync("identity/refresh", null), (5, HttpStatusCode.BadRequest) },
                { () => _client.PostAsJsonAsync("identity/resetlink", email), (2, HttpStatusCode.BadRequest) },
                { () => _client.GetAsync("identity/verification"), (5, HttpStatusCode.BadRequest) },
                { () => _client.PutAsJsonAsync("identity/reset", password), (5, HttpStatusCode.BadRequest) }
            };

            var authorizedEndpoints = new Dictionary<Func<Task<HttpResponseMessage>>, (int Limit, HttpStatusCode Status)>
            {
                { () => _client.PutAsJsonAsync("identity/email", credentials), (5, HttpStatusCode.BadRequest)},
                { () => _client.PutAsJsonAsync("identity/password", passwordUpdate), (5, HttpStatusCode.BadRequest) },
                { () => _client.PostAsync("identity/verificationlink", null), (2, HttpStatusCode.FailedDependency) },
                { () => _client.PutAsJsonAsync("identity/delete", password), (5, HttpStatusCode.BadRequest) }
            };

            foreach (var ip in ipAddresses)
            {
                _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
                _client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);

                foreach (var endpoint in unauthorizedEndpoints)
                {
                    for (int i = 0; i <= endpoint.Value.Limit; i++)
                    {
                        var response = await endpoint.Key();

                        if (i != endpoint.Value.Limit)
                        {
                            Assert.Equal(endpoint.Value.Status, response.StatusCode);
                        }
                        else
                        {
                            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
                        }
                    }
                }
            }

            _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
            _client.DefaultRequestHeaders.Add("X-Forwarded-For", "9.8.7.6");

            var authorize = await IdentityExtensions.RegisterWithCredentialsAsync(_client, "test@test.com", "password");
            Assert.Equal(HttpStatusCode.Created, authorize.StatusCode);

            authorize = await IdentityExtensions.LoginWithCredentialsAsync(_client, "test@test.com", "password");
            Assert.Equal(HttpStatusCode.OK, authorize.StatusCode);

            foreach (var ip in ipAddresses)
            {
                _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
                _client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);

                foreach (var endpoint in authorizedEndpoints)
                {
                    for (int i = 0; i <= endpoint.Value.Limit; i++)
                    {
                        var response = await endpoint.Key();

                        if (i != endpoint.Value.Limit)
                        {
                            Assert.Equal(endpoint.Value.Status, response.StatusCode);
                        }
                        else
                        {
                            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
                        }
                    }
                }
            }
        }
    }
}
