using System.Text;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Models.Requests;

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

            return Ok(new { Id = id, Doc = result.ContentAs<Store>() });
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

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int pageIndex = 0, int pageSize = 10, string filter = null)
    {
        pageIndex = Math.Abs(pageIndex);

        try {
            StringBuilder query = new();
            query.AppendLine($"select * from {nameof(Store)}");

            if (string.IsNullOrWhiteSpace(filter) == false)
                query.AppendLine("where name like '%$filter%'");

            query.AppendLine("limit $limit offset $offset");

            QueryOptions options = new();
            options.Parameter("filter", filter);
            options.Parameter("limit", pageSize);
            options.Parameter("offset", pageIndex * pageSize);

            IQueryResult<Store> queryResult = await _scope.QueryAsync<Store>(query.ToString(), options);

            if (queryResult.MetaData.Status != QueryStatus.Success)
                return StatusCode(500);

            await foreach (var item in queryResult)
            {

            }

            return Ok();
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
    }
}