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
    Task<Product> AddProductAsync(string storeId, Product product);
    Task<Product> EditProductAsync(string storeId, Product product);
    Task<IEnumerable<Product>> GetProductsByStoreIdAsync(string storeId);
    Task DeleteProductById(string storeId, string productId);
}