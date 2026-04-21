namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;



public interface IContractWorkflowService
{
    
    
    string? ValidateServiceRequestCreation(Contract contract);

   
    
    string? ValidateStatusTransition(string currentStatus, string newStatus);
}
