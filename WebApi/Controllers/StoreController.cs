using System.Text;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Models.Requests;
using WebApi.Models.Responses;

namespace WebApi.Controllers;

[ApiController]
[Route("api/store")]
public class StoreController : ControllerBase
{
    private readonly IScope _scope;
    private readonly ICouchbaseCollection _collection;

    public StoreController(IScope scope)
    {
        _scope = scope;
        _collection = scope.CollectionAsync(nameof(Store)).GetAwaiter().GetResult();
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] StoreRequest request)
    {
        Store store = new()
        {
            Name = request.Name,
            Active = request.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        string id = $"{Guid.NewGuid()}";

        try {
            _ = await _collection.InsertAsync(id, store, options => {
                options.Timeout(TimeSpan.FromSeconds(30));
            });
            
            return Created($"store/{id}", new { Id = id, Doc = store });
        }
        catch (DocumentExistsException) {
            return Conflict();
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
    {
        try {
            IGetResult result = await _collection.GetAsync(id);

            return Ok(new StoreResponse { Id = id, Store = result.ContentAs<Store>() });
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditAsync([FromRoute] string id, [FromBody] StoreRequest request)
    {
        try {
            IGetResult result = await _collection.GetAsync(id);
            
            Store store = result.ContentAs<Store>();
            store.Name = request.Name;
            store.Active = request.Active;

            _ = await _collection.ReplaceAsync(id, store);

            return Ok(new { Id = id, Doc = result.ContentAs<Store>() });
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] string id)
    {
        try {
            IExistsResult existsResult = await _collection.ExistsAsync(id);
            
            if (existsResult.Exists == false)
                return NotFound();

            await _collection.RemoveAsync(id);
            return NoContent();
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync()
    {
        try {
            IQueryResult<StoreResponse> queryResult = await _scope.QueryAsync<StoreResponse>($"select META().id AS id, * from {nameof(Store)}", new QueryOptions());

            if (queryResult.MetaData.Status != QueryStatus.Success)
                return StatusCode(500);
            
            return Ok(await queryResult.ToListAsync());
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int pageIndex = 0, int pageSize = 10, string filter = null)
    {
        pageIndex = Math.Abs(pageIndex);

        try {
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

            if (queryResult.MetaData.Status != QueryStatus.Success)
                return StatusCode(500);
            
            return Ok(await queryResult.ToListAsync());
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }
}