using GeminiRAG.Core.Entities;
using GeminiRAG.Core.Interfaces;
using GeminiRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeminiRAG.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly ApplicationDbContext _context;

    public StoreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Store>> GetStoresByUserIdAsync(Guid userId)
    {
        return await _context.Stores
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Store?> GetStoreByIdAsync(Guid storeId)
    {
        return await _context.Stores.FindAsync(storeId);
    }

    public async Task<Store?> GetStoreByNameAsync(Guid userId, string name)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Name == name);
    }

    public async Task<Store> CreateStoreAsync(Store store)
    {
        _context.Stores.Add(store);
        await _context.SaveChangesAsync();
        return store;
    }

    public async Task<bool> DeleteStoreAsync(Guid storeId)
    {
        var store = await _context.Stores.FindAsync(storeId);
        if (store == null)
        {
            return false;
        }

        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StoreExistsAsync(Guid storeId)
    {
        return await _context.Stores.AnyAsync(s => s.Id == storeId);
    }
}
