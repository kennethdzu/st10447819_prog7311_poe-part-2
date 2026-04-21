using TechMove.Glms.Web.Models;
using TechMove.Glms.Web.Services;
using Xunit;

namespace TechMove.Glms.Tests;

// Tests for the ContractWorkflowService to validate ServiceRequest and status transitions
public class WorkflowValidationTests
{
    private readonly ContractWorkflowService _service = new ContractWorkflowService();


    [Theory]
    [InlineData("Expired")]
    [InlineData("On Hold")]
    [InlineData("Draft")]
    public void ValidateServiceRequestCreation_BlockedStatus_ReturnsErrorMessage(string status)
    {
        var contract = new FreightContract { Status = status };

        var error = _service.ValidateServiceRequestCreation(contract);

        Assert.NotNull(error);
        Assert.NotEmpty(error);
    }

    [Theory]
    [InlineData("Active")]
    public void ValidateServiceRequestCreation_AllowedStatus_ReturnsNull(string status)
    {
        var contract = new FreightContract { Status = status };

        var error = _service.ValidateServiceRequestCreation(contract);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateServiceRequestCreation_NullContract_ReturnsErrorMessage()
    {
        var error = _service.ValidateServiceRequestCreation(null!);

        Assert.NotNull(error);
    }


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
