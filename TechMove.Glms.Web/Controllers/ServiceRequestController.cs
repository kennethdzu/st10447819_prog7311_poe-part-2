using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string _token;

        public ServiceRequestController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            HttpClient client = await GetClientAsync();
            List<ServiceRequest> requests = await client.GetFromJsonAsync<List<ServiceRequest>>("/api/servicerequests") ?? new List<ServiceRequest>();
            return View(requests);
        }

        public async Task<IActionResult> Create(int? contractId)
        {
            HttpClient client = await GetClientAsync();
            List<Contract> contracts = await client.GetFromJsonAsync<List<Contract>>("/api/contracts") ?? new List<Contract>();
            ViewBag.Contracts = contracts;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,Status")] ServiceRequest serviceRequest, decimal CostUsd)
        {
            if (ModelState.IsValid)
            {
                HttpClient client = await GetClientAsync();
                
                var payload = new
                {
                    Request = serviceRequest,
                    CostUsd = CostUsd
                };

                var response = await client.PostAsJsonAsync("/api/servicerequests", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "API Error: " + await response.Content.ReadAsStringAsync());
                }
            }

            HttpClient refreshClient = await GetClientAsync();
            List<Contract> cli = await refreshClient.GetFromJsonAsync<List<Contract>>("/api/contracts") ?? new List<Contract>();
            ViewBag.Contracts = cli;
            return View(serviceRequest);
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
