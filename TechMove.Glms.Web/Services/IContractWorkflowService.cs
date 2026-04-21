namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;


/// Handles my apps contract rules and status checks.

public interface IContractWorkflowService
{
    
    /// This validates whether a new ServiceRequest can be raised against the given contract.
    /// And it will returns null when the request is permitted and it returns an error message when blocked.
    
    string? ValidateServiceRequestCreation(Contract contract);

   
    /// This validates whether transitioning a contract from one status to another is fine/allowed.
    /// And it returns null when the transition is valid and returns an error message when blocked.
    
    string? ValidateStatusTransition(string currentStatus, string newStatus);
}
