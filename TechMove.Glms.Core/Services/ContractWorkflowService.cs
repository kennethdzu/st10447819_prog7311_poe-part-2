using System;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Services
{
    public class ContractWorkflowService : IContractWorkflowService
    {
        public string ValidateServiceRequestCreation(Contract contract)
        {
            if (contract == null)
            {
                return "The associated contract could not be found.";
            }

            if (!contract.Status?.Trim().Equals("Active", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "A Service Request cannot be created for a contract with status '" + contract.Status + "'. Only Active contracts may receive new requests.";
            }

            return null; 
        }

        public string ValidateStatusTransition(string currentStatus, string newStatus)
        {
            if (currentStatus == "Draft")
            {
                if (newStatus == "Active" || newStatus == "On Hold")
                {
                    return null;
                }
            }
            else if (currentStatus == "Active")
            {
                if (newStatus == "On Hold" || newStatus == "Expired")
                {
                    return null;
                }
            }
            else if (currentStatus == "On Hold")
            {
                if (newStatus == "Active" || newStatus == "Expired")
                {
                    return null;
                }
            }
            else if (currentStatus == "Expired")
            {
                return "Cannot transition from Expired.";
            }
            
            return "Cannot transition a contract from '" + currentStatus + "' to '" + newStatus + "'.";
        }
    }
}
