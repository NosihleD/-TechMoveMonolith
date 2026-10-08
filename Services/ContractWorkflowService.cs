using TechMove.Models;

namespace TechMove.Services;

public class ContractWorkflowService
{
    public bool IsContractExpired(Contract contract) =>
        (contract.EndDate != default && contract.EndDate.Date < DateTime.Today) ||
        string.Equals(contract.Status, ContractStatuses.Expired, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(contract.Status, ContractStatuses.OnHold, StringComparison.OrdinalIgnoreCase);

    public bool CanCreateServiceRequest(Contract contract) => !IsContractExpired(contract);

    public void EnsureCanCreateServiceRequest(Contract contract)
    {
        if (!CanCreateServiceRequest(contract))
            throw new InvalidOperationException($"A service request cannot be created because the contract is '{contract.Status}' or has expired.");
    }

    public void ValidateContractDates(Contract contract)
    {
        if (contract.EndDate < contract.StartDate)
            throw new InvalidOperationException("Contract end date cannot be before the start date.");
    }
}
