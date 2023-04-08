using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace Api.Tests
{
    public abstract class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        protected readonly ApiWebApplicationFactory _factory;
        protected readonly HttpClient _client;
        protected readonly IConfiguration _configuration;

        public IntegrationTest(ApiWebApplicationFactory fixture)
        {
            _factory = fixture;
            _client = _factory.CreateClient();
            _configuration = _factory.Configuration;
        }

        public async Task ResetDatabse()
        {
            await using var npgsqlConnection = new NpgsqlConnection(_configuration.GetConnectionString("Database") ?? "");
            await npgsqlConnection.OpenAsync();

            var respawner = await Respawner.CreateAsync(npgsqlConnection, new RespawnerOptions
            {
                SchemasToInclude = new[]
                {
                    "identity"
                },
                TablesToIgnore = new Table[]
                {
                    "provider"
                },
                DbAdapter = DbAdapter.Postgres
            });

            await respawner.ResetAsync(npgsqlConnection);
        }
    }
}