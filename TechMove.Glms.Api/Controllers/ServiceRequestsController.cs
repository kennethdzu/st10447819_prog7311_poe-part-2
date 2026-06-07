using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Glms.Core.Repositories;
using TechMove.Glms.Web.Models;
using TechMove.Glms.Web.Services;

namespace TechMove.Glms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IRepository<ServiceRequest> _serviceRequestRepo;
        private readonly IRepository<Contract> _contractRepo;
        private readonly IContractWorkflowService _workflowService;
        private readonly ICurrencyConversionStrategy _currencyStrategy;

        public ServiceRequestsController(
            IRepository<ServiceRequest> serviceRequestRepo,
            IRepository<Contract> contractRepo,
            IContractWorkflowService workflowService,
            ICurrencyConversionStrategy currencyStrategy)
        {
            _serviceRequestRepo = serviceRequestRepo;
            _contractRepo = contractRepo;
            _workflowService = workflowService;
            _currencyStrategy = currencyStrategy;
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceRequests()
        {
            var requests = await _serviceRequestRepo.GetAllAsync();
            return Ok(requests);
        }

        public class CreateServiceRequestDto
        {
            public ServiceRequest Request { get; set; }
            public decimal CostUsd { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
        {
            if (dto == null || dto.Request == null)
            {
                return BadRequest("Invalid payload.");
            }

            var contract = await _contractRepo.GetByIdAsync(dto.Request.ContractId);
            if (contract == null)
            {
                return BadRequest("Contract not found.");
            }

            string workflowError = _workflowService.ValidateServiceRequestCreation(contract);
            if (workflowError != null)
            {
                return BadRequest(workflowError);
            }

            try
            {
                dto.Request.Cost = await _currencyStrategy.ConvertUsdToZarAsync(dto.CostUsd);
            }
            catch (Exception ex)
            {
                return BadRequest("Currency Conversion Failed: " + ex.Message);
            }

            await _serviceRequestRepo.AddAsync(dto.Request);
            await _serviceRequestRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceRequests), new { id = dto.Request.Id }, dto.Request);
        }
    }
}
