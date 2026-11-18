using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Models;

namespace TechMove.Api.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ApplicationDbContext _context;

    public ContractRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contract>> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (startDate.HasValue)
{
    var fromDate = startDate.Value.Date;

    query = query.Where(c => c.EndDate >= fromDate);
}

if (endDate.HasValue)
{
    var toDate = endDate.Value.Date.AddDays(1);

    query = query.Where(c => c.StartDate < toDate);
}
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        return await query
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<Contract?> GetByIdAsync(int id)
    {
        return await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.ContractId == id);
    }

    public async Task<Contract> AddAsync(Contract contract)
    {
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return contract;
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractId == id);

        if (contract == null)
            return false;

        contract.Status = status;

        await _context.SaveChangesAsync();
        return true;
    }
}