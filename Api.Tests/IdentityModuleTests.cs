using Api.Modules.Identity.Models;
using System.Net.Http.Json;
using Xunit;

namespace Api.Tests
{
    public class IdentityModuleTests : IntegrationTest
    {
        public IdentityModuleTests(ApiWebApplicationFactory fixture) 
            : base(fixture) { }

        [Fact]
        public async Task TestPostRegister()
        {
            await ResetDatabse();

            var response = await Extensions.RegisterWithCredentialsAsync(_client, "", "");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            response = await Extensions.RegisterWithCredentialsAsync(_client, "test@test.com", "password");

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TestPostRegisterExists()
        {
            var response = await Extensions.RegisterWithCredentialsAsync(_client, "test@test.com", "password");

            Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task TestPostLogin()
        {
            var response = await Extensions.LoginWithCredentialsAsync(_client, "", "");

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            response = await Extensions.LoginWithCredentialsAsync(_client, "test@test.com", "password");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestPutPassword()
        {
            var passwordUpdate = new PasswordUpdateModel();
            var response = await _client.PutAsJsonAsync<PasswordUpdateModel>("identity/password", passwordUpdate);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

            await Extensions.LoginWithCredentialsAsync(_client, "test@test.com", "password");

            response = await _client.PutAsJsonAsync<PasswordUpdateModel>("identity/password", passwordUpdate);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            passwordUpdate.Password = "P4ssW0rd!";
            passwordUpdate.CurrentPassword = "password";
            response = await _client.PutAsJsonAsync<PasswordUpdateModel>("identity/password", passwordUpdate);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
