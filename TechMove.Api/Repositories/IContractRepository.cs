using TechMove.Models;

namespace TechMove.Api.Repositories;

public interface IContractRepository
{
    Task<IEnumerable<Contract>> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status);

    Task<Contract?> GetByIdAsync(int id);

    Task<Contract> AddAsync(Contract contract);

    Task<bool> UpdateStatusAsync(int id, string status);
}