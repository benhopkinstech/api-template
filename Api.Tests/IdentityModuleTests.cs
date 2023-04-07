using Xunit;

namespace Api.Tests
{
    public class IdentityModuleTests : IClassFixture<ApiWebApplicationFactory>
    {
        readonly HttpClient _client;

        public IdentityModuleTests(ApiWebApplicationFactory application)
        {
            _client = application.CreateClient();
        }
    }
}
