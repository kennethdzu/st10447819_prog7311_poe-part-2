using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Glms.Web.Data;
using TechMove.Glms.Web.Models;
using TechMove.Glms.Web.Services;

namespace TechMove.Glms.Web.Controllers
{
    public class ContractController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FileValidationService _fileValidator;
        private readonly IWebHostEnvironment _env;
        private readonly IContractFactory _contractFactory;
        private string _token;

        public ContractController(
            IHttpClientFactory httpClientFactory,
            FileValidationService validator,
            IWebHostEnvironment environment,
            IContractFactory factory)
        {
            _httpClientFactory = httpClientFactory;
            _fileValidator = validator;
            _env = environment;
            _contractFactory = factory;
        }

        private async Task<HttpClient> GetClientAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient("ApiClient");
            
            // In a real app, this would be retrieved from user session/cookie after login.
            // For this prototype, we authenticate directly to get a service token.
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

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string status)
        {
            HttpClient client = await GetClientAsync();
            
            string query = $"/api/contracts?status={status}";
            List<Contract> contracts = await client.GetFromJsonAsync<List<Contract>>(query);

            if (contracts != null)
            {
                if (startDate.HasValue)
                {
                    contracts = contracts.Where(c => c.StartDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    contracts = contracts.Where(c => c.EndDate <= endDate.Value).ToList();
                }
            }
            
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(contracts ?? new List<Contract>());
        }

        public async Task<IActionResult> Create()
        {
            HttpClient client = await GetClientAsync();
            List<Client> clients = await client.GetFromJsonAsync<List<Client>>("/api/clients") ?? new List<Client>();
            ViewBag.Clients = clients;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateViewModel contract, IFormFile agreementPdf, string ContractType)
        {
            Contract newContract;
            HttpClient client = await GetClientAsync();

            try
            {
                newContract = _contractFactory.Create(ContractType);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Clients = await client.GetFromJsonAsync<List<Client>>("/api/clients") ?? new List<Client>();
                return View(contract);
            }

            newContract.ClientId = contract.ClientId;
            newContract.StartDate = contract.StartDate;
            newContract.EndDate = contract.EndDate;
            newContract.Status = contract.Status;
            newContract.ServiceLevel = contract.ServiceLevel;

            if (newContract is FreightContract freight)
            {
                freight.WeightLimit = contract.WeightLimit.GetValueOrDefault();
                freight.Route = contract.Route;
            }
            else if (newContract is WarehousingContract warehouse)
            {
                warehouse.Capacity = contract.Capacity.GetValueOrDefault();
                warehouse.TemperatureZone = contract.TemperatureZone;
            }
            else if (newContract is LastMileContract lastMile)
            {
                lastMile.DeliveryRadius = contract.DeliveryRadius.GetValueOrDefault();
            }

            if (agreementPdf != null)
            {
                if (!_fileValidator.IsValidPdf(agreementPdf))
                {
                    ModelState.AddModelError("SignedAgreementPdfPath", "Only genuine PDF files are accepted for the Signed Agreement.");
                    ViewBag.Clients = await client.GetFromJsonAsync<List<Client>>("/api/clients") ?? new List<Client>();
                    return View(contract);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + agreementPdf.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await agreementPdf.CopyToAsync(fileStream);
                }

                newContract.SignedAgreementPdfPath = "/uploads/contracts/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                var response = await client.PostAsJsonAsync("/api/contracts", newContract);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "API Error: " + await response.Content.ReadAsStringAsync());
                }
            }

            ViewBag.Clients = await client.GetFromJsonAsync<List<Client>>("/api/clients") ?? new List<Client>();
            return View(contract);
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            HttpClient client = await GetClientAsync();
            var contract = await client.GetFromJsonAsync<Contract>($"/api/contracts/{id}");
            
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPdfPath))
            {
                return NotFound("Agreement file not found.");
            }

            string filePath = Path.Combine(_env.WebRootPath, contract.SignedAgreementPdfPath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File does not exist on disk.");
            }

            MemoryStream memory = new MemoryStream();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/pdf", Path.GetFileName(filePath));
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
