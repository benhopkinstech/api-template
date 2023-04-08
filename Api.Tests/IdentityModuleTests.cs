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

            var credentials = new CredentialsModel();
            var response = await _client.PostAsJsonAsync<CredentialsModel>("identity/register", credentials);

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

            credentials.Email = "test@test.com";
            credentials.Password = "password";
            response = await _client.PostAsJsonAsync<CredentialsModel>("identity/register", credentials);

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created);
        }
    }
}
