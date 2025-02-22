using System.Text;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Microsoft.OpenApi.Writers;
using WebApi.Models;
using WebApi.Models.Responses;

namespace WebApi.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly IScope _scope;
    private readonly ICouchbaseCollection _collection;

    public StoreRepository(IScope scope)
    {
        _scope = scope;
        _collection = scope.CollectionAsync(nameof(Store)).GetAwaiter().GetResult();
    }

    public async Task<StoreResponse> AddAsync(Store store)
    {
        string id = $"{Guid.NewGuid()}";
        _ = await _collection.InsertAsync(id, store, options => options.Timeout(TimeSpan.FromSeconds(30)));
        
        return new StoreResponse { Id = id, Store = store };
    }

    public async Task<StoreResponse> GetByIdAsync(string id)
    {
        IGetResult result = await _collection.GetAsync(id, options => options.AsReadOnly());

        return new StoreResponse { Id = id, Store = result.ContentAs<Store>() };
    }

    public async Task<StoreResponse> UpdateAsync(string id, Store store)
    {
        _ = await _collection.ReplaceAsync(id, store);

        return new StoreResponse { Id = id, Store = store };
    }

    public async Task DeleteAsync(string id)
    {
        IGetResult result = await _collection.GetAsync(id);
        
        await _collection.RemoveAsync(id);
    }

    public async Task<IEnumerable<StoreResponse>> GetAllAsync()
    {
        IQueryResult<StoreResponse> queryResult = await _scope.QueryAsync<StoreResponse>(
            $"select META().id AS id, * from {nameof(Store)}");

        if (queryResult.MetaData.Status == QueryStatus.Success)
            return await queryResult.ToListAsync();
        
        return [];
    }

    public async Task<IEnumerable<StoreResponse>> GetAsync(int pageIndex, int pageSize, string filter)
    {
        StringBuilder query = new();
        QueryOptions options = new();
        options.AsReadOnly();
            
        query.Append($"select META().id AS id, * from {nameof(Store)} ");

        if (string.IsNullOrWhiteSpace(filter) == false)
        {
            query.Append("where name like $filter ");
            options.Parameter("filter", $"%{filter}%");
        }

        query.Append("order by createdAt desc ");
        query.Append("limit $limit offset $offset ");
            
        options.Parameter("limit", pageSize);
        options.Parameter("offset", pageIndex * pageSize);

        IQueryResult<StoreResponse> queryResult = await _scope.QueryAsync<StoreResponse>(query.ToString(), options);

        if (queryResult.MetaData.Status == QueryStatus.Success)
            return await queryResult.ToListAsync();
            
        return [];
    }

    public async Task<Product> AddProductAsync(string storeId, Product product)
    {
        product.Id = $"{Guid.NewGuid()}";
        
        await _collection.MutateInAsync(storeId, specs =>
            specs.ArrayAppend("products", [product]));
        
        return product;
    }
    
    public async Task<Product> EditProductAsync(string storeId, Product product)
    {
        StoreResponse current = await GetByIdAsync(storeId);
        int productIndex = current.Store.Products.FindIndex(p => p.Id == product.Id);

        if (productIndex < 0)
            throw new DocumentNotFoundException();
        
        await _collection.MutateInAsync(storeId, specs =>
            specs.Replace($"products[{productIndex}]", product));
        
        return product;
    }

    public async Task<IEnumerable<Product>> GetProductsByStoreIdAsync(string storeId)
    {
        ILookupInResult result = await _collection.LookupInAsync(storeId, specs =>
            specs.Get("products"));

        return result.ContentAs<List<Product>>(0);
    }

    public async Task DeleteProductById(string storeId, string productId)
    {
        StoreResponse current = await GetByIdAsync(storeId);
        int productIndex = current.Store.Products.FindIndex(p => p.Id == productId);
        
        if (productIndex < 0)
            throw new DocumentNotFoundException();
        
        await _collection.MutateInAsync(storeId, specs =>
            specs.Remove($"products[{productIndex}]"));
    }
}