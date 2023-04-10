using Api.Modules.Identity.Data;
using Microsoft.EntityFrameworkCore;
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
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Database") ?? "");
            await connection.OpenAsync();

            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
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

            await respawner.ResetAsync(connection);
        }

        public IdentityContext CreateIdentityContext()
        {
            var options = new DbContextOptionsBuilder<IdentityContext>()
                .UseNpgsql(_configuration.GetConnectionString("Database"))
                .Options;

            var context = new IdentityContext(options);

            return context;
        }
    }
}