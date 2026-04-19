using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Glms.Web.Data;
using TechMove.Glms.Web.Models;
using TechMove.Glms.Web.Services;

namespace TechMove.Glms.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        AppDbContext db;
        ICurrencyConversionStrategy currencyStrategy;
        IContractWorkflowService workflowService;

        public ServiceRequestController(
            AppDbContext context,
            ICurrencyConversionStrategy strategy,
            IContractWorkflowService service)
        {
            db = context;
            currencyStrategy = strategy;
            workflowService = service;
        }

        public async Task<IActionResult> Index()
        {
            List<ServiceRequest> requests = await db.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> Create(int? contractId)
        {
            List<Contract> contracts = await db.Contracts
                .Where(c => c.Status != "Expired" && c.Status != "On Hold")
                .ToListAsync();
            ViewBag.Contracts = contracts;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,Status")] ServiceRequest serviceRequest, decimal CostUsd)
        {
            Contract contract = await db.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

            string workflowError = workflowService.ValidateServiceRequestCreation(contract);
            
            if (workflowError != null)
            {
                ModelState.AddModelError("ContractId", workflowError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serviceRequest.Cost = await currencyStrategy.ConvertUsdToZarAsync(CostUsd);

                    db.Add(serviceRequest);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CostUsd", "Currency Conversion Failed: " + ex.Message);
                }
            }

            List<Contract> cli = await db.Contracts.ToListAsync();
            ViewBag.Contracts = cli;
            return View(serviceRequest);
        }
    }
}
