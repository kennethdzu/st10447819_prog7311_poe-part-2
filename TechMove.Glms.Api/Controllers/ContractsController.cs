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
    public class ContractsController : ControllerBase
    {
        private readonly IRepository<Contract> _contractRepo;
        private readonly IContractWorkflowService _workflowService;

        public ContractsController(IRepository<Contract> contractRepo, IContractWorkflowService workflowService)
        {
            _contractRepo = contractRepo;
            _workflowService = workflowService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContracts([FromQuery] string status = null)
        {
            IEnumerable<Contract> contracts;
            if (!string.IsNullOrEmpty(status))
            {
                contracts = await _contractRepo.FindAsync(c => c.Status == status);
            }
            else
            {
                contracts = await _contractRepo.GetAllAsync();
            }

            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractRepo.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            return Ok(contract);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] Contract contract)
        {
            if (contract == null)
            {
                return BadRequest("Contract cannot be null.");
            }

            await _contractRepo.AddAsync(contract);
            await _contractRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContracts), new { id = contract.Id }, contract);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var contract = await _contractRepo.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound("Contract not found.");
            }

            string error = _workflowService.ValidateStatusTransition(contract.Status, newStatus);
            if (error != null)
            {
                return BadRequest(error);
            }

            contract.Status = newStatus;
            _contractRepo.Update(contract);
            await _contractRepo.SaveChangesAsync();

            return Ok(contract);
        }
    }
}
