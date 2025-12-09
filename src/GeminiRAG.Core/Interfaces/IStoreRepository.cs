using GeminiRAG.Core.Entities;

namespace GeminiRAG.Core.Interfaces;

public interface IStoreRepository
{
    Task<List<Store>> GetStoresByUserIdAsync(Guid userId);
    Task<Store?> GetStoreByIdAsync(Guid storeId);
    Task<Store?> GetStoreByNameAsync(Guid userId, string name);
    Task<Store> CreateStoreAsync(Store store);
    Task<bool> DeleteStoreAsync(Guid storeId);
    Task<bool> StoreExistsAsync(Guid storeId);
}
