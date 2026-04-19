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
        AppDbContext db;
        FileValidationService fileValidator;
        IWebHostEnvironment env;
        IContractFactory contractFactory;

        public ContractController(
            AppDbContext context,
            FileValidationService validator,
            IWebHostEnvironment environment,
            IContractFactory factory)
        {
            db = context;
            fileValidator = validator;
            env = environment;
            contractFactory = factory;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string status)
        {
            IQueryable<Contract> query = db.Contracts.Include(c => c.Client).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            List<Contract> contracts = await query.OrderByDescending(c => c.Id).ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(contracts);
        }

        public async Task<IActionResult> Create()
        {
            List<Client> clients = await db.Clients.ToListAsync();
            ViewBag.Clients = clients;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateViewModel contract, IFormFile agreementPdf, string ContractType)
        {
            Contract newContract;

            try
            {
                newContract = contractFactory.Create(ContractType);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                List<Client> li = await db.Clients.ToListAsync();
                ViewBag.Clients = li;
                return View(contract);
            }

            newContract.ClientId = contract.ClientId;
            newContract.StartDate = contract.StartDate;
            newContract.EndDate = contract.EndDate;
            newContract.Status = contract.Status;
            newContract.ServiceLevel = contract.ServiceLevel;

            if (agreementPdf != null)
            {
                if (!fileValidator.IsValidPdf(agreementPdf))
                {
                    ModelState.AddModelError("SignedAgreementPdfPath", "Only genuine PDF files are accepted for the Signed Agreement (the file must have a .pdf extension, application/pdf MIME type, and a valid PDF header).");
                    List<Client> li = await db.Clients.ToListAsync();
                    ViewBag.Clients = li;
                    return View(contract);
                }

                string uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "contracts");
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
                db.Add(newContract);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            List<Client> cli = await db.Clients.ToListAsync();
            ViewBag.Clients = cli;
            return View(contract);
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            Contract contract = await db.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPdfPath))
            {
                return NotFound("Agreement file not found.");
            }

            string filePath = Path.Combine(env.WebRootPath, contract.SignedAgreementPdfPath.TrimStart('/'));
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
    }
}
