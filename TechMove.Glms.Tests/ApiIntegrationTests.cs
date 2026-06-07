using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechMove.Glms.Web.Data;
using TechMove.Glms.Web.Models;
using Xunit;

namespace TechMove.Glms.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace production DbContext with a clean, isolated SQLite test DB
                    ServiceDescriptor descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite("Data Source=integration_test.db"));

                    ServiceProvider sp = services.BuildServiceProvider();
                    using (IServiceScope scope = sp.CreateScope())
                    {
                        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        db.Database.EnsureCreated();
                    }
                });
            });
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            HttpClient client = _factory.CreateClient();
            var authResponse = await client.PostAsync("/api/auth/login", null);
            authResponse.EnsureSuccessStatusCode();
            TokenResponse tokenResult = await authResponse.Content.ReadFromJsonAsync<TokenResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            return client;
        }

        [Fact]
        public async Task GetContracts_ReturnsSuccessStatusCodeAndJsonContentType()
        {
            HttpClient client = await GetAuthenticatedClientAsync();

            HttpResponseMessage response = await client.GetAsync("/api/contracts");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task PostClient_ThenGetClients_ContainsCreatedClient()
        {
            HttpClient client = await GetAuthenticatedClientAsync();

            Client newClient = new Client
            {
                Name = "Acme Logistics",
                ContactDetails = "acme@logistics.co.za",
                Region = "Gauteng"
            };

            HttpResponseMessage postResponse = await client.PostAsJsonAsync("/api/clients", newClient);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            List<Client> clients = await client.GetFromJsonAsync<List<Client>>("/api/clients");
            Assert.NotNull(clients);
            Assert.Contains(clients, c => c.Name == "Acme Logistics");
        }

        [Fact]
        public async Task PostContract_ThenGetContracts_ContainsCreatedContract()
        {
            HttpClient client = await GetAuthenticatedClientAsync();

            // Create a client to satisfy the foreign key constraint
            Client seedClient = new Client
            {
                Name = "Test Freight Client",
                ContactDetails = "freight@test.co.za",
                Region = "Western Cape"
            };

            HttpResponseMessage clientPost = await client.PostAsJsonAsync("/api/clients", seedClient);
            Assert.Equal(HttpStatusCode.Created, clientPost.StatusCode);
            Client createdClient = await clientPost.Content.ReadFromJsonAsync<Client>();

            // Now create a contract linked to that client
            FreightContract newContract = new FreightContract
            {
                ClientId = createdClient.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = "Active",
                ServiceLevel = "Premium",
                WeightLimit = 5000,
                Route = "Cape Town to Johannesburg"
            };

            // Serialize manually so the $contractType polymorphic discriminator is included
            string contractJson = System.Text.Json.JsonSerializer.Serialize<Contract>(newContract);
            System.Net.Http.StringContent contractContent = new System.Net.Http.StringContent(
                contractJson,
                System.Text.Encoding.UTF8,
                "application/json");

            HttpResponseMessage contractPost = await client.PostAsync("/api/contracts", contractContent);
            Assert.Equal(HttpStatusCode.Created, contractPost.StatusCode);

            // Read back and verify
            List<Contract> contracts = await client.GetFromJsonAsync<List<Contract>>("/api/contracts");
            Assert.NotNull(contracts);
            Assert.Contains(contracts, c => c.ClientId == createdClient.Id && c.Status == "Active");
        }

        [Fact]
        public async Task GetContracts_WithStatusFilter_ReturnsOnlyMatchingStatus()
        {
            HttpClient client = await GetAuthenticatedClientAsync();

            HttpResponseMessage response = await client.GetAsync("/api/contracts?status=Active");

            response.EnsureSuccessStatusCode();
            List<Contract> contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
            Assert.All(contracts, c => Assert.Equal("Active", c.Status));
        }

        [Fact]
        public async Task GetContract_WithInvalidId_ReturnsNotFound()
        {
            HttpClient client = await GetAuthenticatedClientAsync();

            HttpResponseMessage response = await client.GetAsync("/api/contracts/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
