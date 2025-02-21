using WebApi.Models;
using WebApi.Models.Responses;

namespace WebApi.Repositories;

public interface IStoreRepository
{
    Task<StoreResponse> AddAsync(Store store);
    Task<StoreResponse> GetByIdAsync(string id);
    Task<StoreResponse> UpdateAsync(string id, Store store);
    Task DeleteAsync(string id);
    Task<IEnumerable<StoreResponse>> GetAllAsync();
    Task<IEnumerable<StoreResponse>> GetAsync(int pageIndex, int pageSize, string filter);
}