namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;

/// <summary>
/// Handles contract rules and status checks.
/// </summary>
public interface IContractWorkflowService
{
    /// <summary>
    /// Validates whether a new ServiceRequest can be raised against the given contract.
    /// Returns null when the request is permitted; returns an error message when blocked.
    /// </summary>
    string? ValidateServiceRequestCreation(Contract contract);

    /// <summary>
    /// Validates whether transitioning a contract from one status to another is allowed.
    /// Returns null when the transition is valid; returns an error message when blocked.
    /// </summary>
    string? ValidateStatusTransition(string currentStatus, string newStatus);
}
