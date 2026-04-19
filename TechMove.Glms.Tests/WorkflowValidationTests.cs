using TechMove.Glms.Web.Models;
using TechMove.Glms.Web.Services;
using Xunit;

namespace TechMove.Glms.Tests;

/// <summary>
/// Tests for the ContractWorkflowService — the Observer-pattern implementation
/// that validates whether a ServiceRequest can be raised against a given contract,
/// and whether a status transition is permitted.
/// </summary>
public class WorkflowValidationTests
{
    private readonly ContractWorkflowService _service = new ContractWorkflowService();

    // ── ServiceRequest creation validation ────────────────────────────────────

    [Theory]
    [InlineData("Expired")]
    [InlineData("On Hold")]
    public void ValidateServiceRequestCreation_BlockedStatus_ReturnsErrorMessage(string status)
    {
        // Arrange — contract is in a status that should block new requests.
        var contract = new FreightContract { Status = status };

        // Act
        var error = _service.ValidateServiceRequestCreation(contract);

        // Assert — must return a non-null, non-empty error message.
        Assert.NotNull(error);
        Assert.NotEmpty(error);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Draft")]
    public void ValidateServiceRequestCreation_AllowedStatus_ReturnsNull(string status)
    {
        // Arrange — Active and Draft contracts should allow new service requests.
        var contract = new FreightContract { Status = status };

        // Act
        var error = _service.ValidateServiceRequestCreation(contract);

        // Assert — null means "no error, proceed".
        Assert.Null(error);
    }

    [Fact]
    public void ValidateServiceRequestCreation_NullContract_ReturnsErrorMessage()
    {
        // Arrange — contract could not be found (e.g., invalid contractId in form).
        // Act
        var error = _service.ValidateServiceRequestCreation(null!);

        // Assert
        Assert.NotNull(error);
    }

    // ── Status transition validation ──────────────────────────────────────────

    [Theory]
    [InlineData("Draft",   "Active")]
    [InlineData("Draft",   "On Hold")]
    [InlineData("Active",  "On Hold")]
    [InlineData("Active",  "Expired")]
    [InlineData("On Hold", "Active")]
    [InlineData("On Hold", "Expired")]
    public void ValidateStatusTransition_ValidTransition_ReturnsNull(string from, string to)
    {
        var error = _service.ValidateStatusTransition(from, to);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("Expired", "Active")]   // Expired is terminal
    [InlineData("Expired", "Draft")]    // Expired is terminal
    [InlineData("Draft",   "Expired")]  // Cannot jump directly Draft → Expired
    public void ValidateStatusTransition_InvalidTransition_ReturnsErrorMessage(string from, string to)
    {
        var error = _service.ValidateStatusTransition(from, to);
        Assert.NotNull(error);
        Assert.NotEmpty(error);
    }
}
