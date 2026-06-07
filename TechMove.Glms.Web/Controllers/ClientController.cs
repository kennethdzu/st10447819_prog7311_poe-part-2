using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string _token;

        public ClientController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task<HttpClient> GetClientAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient("ApiClient");
            
            if (string.IsNullOrEmpty(_token))
            {
                var authResponse = await client.PostAsync("/api/auth/login", null);
                if (authResponse.IsSuccessStatusCode)
                {
                    var tokenResult = await authResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    _token = tokenResult?.Token;
                }
            }

            if (!string.IsNullOrEmpty(_token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
            return client;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                HttpClient httpClient = await GetClientAsync();
                var response = await httpClient.PostAsJsonAsync("/api/clients", client);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Contract");
                }
                else
                {
                    ModelState.AddModelError("", "API Error: " + await response.Content.ReadAsStringAsync());
                }
            }
            return View(client);
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
