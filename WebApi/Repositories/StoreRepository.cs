using System.Text;
using Couchbase.KeyValue;
using Couchbase.Query;
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
        _ = await _collection.InsertAsync(id, store, options => { options.Timeout(TimeSpan.FromSeconds(30)); });
        
        return new StoreResponse { Id = id, Store = store };
    }

    public async Task<StoreResponse> GetByIdAsync(string id)
    {
        IGetResult result = await _collection.GetAsync(id);

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
            $"select META().id AS id, * from {nameof(Store)}", new QueryOptions());

        if (queryResult.MetaData.Status == QueryStatus.Success)
            return await queryResult.ToListAsync();
        
        return [];
    }

    public async Task<IEnumerable<StoreResponse>> GetAsync(int pageIndex, int pageSize, string filter)
    {
        StringBuilder query = new();
        QueryOptions options = new();
            
        query.Append($"select META().id AS id, * from {nameof(Store)} ");

        if (string.IsNullOrWhiteSpace(filter) == false)
        {
            query.Append("where name like $filter ");
            options.Parameter("filter", $"%{filter}%");
        }

        query.Append("limit $limit offset $offset ");
            
        options.Parameter("limit", pageSize);
        options.Parameter("offset", pageIndex * pageSize);

        IQueryResult<StoreResponse> queryResult = await _scope.QueryAsync<StoreResponse>(query.ToString(), options);

        if (queryResult.MetaData.Status == QueryStatus.Success)
            return await queryResult.ToListAsync();
            
        return [];
    }
}