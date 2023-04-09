using Api.Modules.Identity.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Api.Tests
{
    public static class Extensions
    {
        public static async Task<HttpResponseMessage> RegisterWithCredentialsAsync(HttpClient client, string email, string password)
        {
            var credentials = new CredentialsModel()
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync<CredentialsModel>("identity/register", credentials);

            return response;
        }

        public static async Task<HttpResponseMessage> LoginWithCredentialsAsync(HttpClient client, string email, string password)
        {
            var credentials = new CredentialsModel()
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync<CredentialsModel>("identity/login", credentials);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = await response.Content.ReadAsStringAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return response;
        }
    }
}
