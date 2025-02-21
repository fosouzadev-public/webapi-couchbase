using System.Text;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Models.Requests;
using WebApi.Models.Responses;
using WebApi.Repositories;

namespace WebApi.Controllers;

[ApiController]
[Route("api/store")]
public class StoreController(IStoreRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] StoreRequest request)
    {
        Store store = new()
        {
            Name = request.Name,
            Active = request.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            StoreResponse response = await repository.AddAsync(store);

            return Created($"store/{response.Id}", response);
        }
        catch (DocumentExistsException)
        {
            return Conflict();
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
    {
        try
        {
            return Ok(await repository.GetByIdAsync(id));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditAsync([FromRoute] string id, [FromBody] StoreRequest request)
    {
        try
        {
            StoreResponse current = await repository.GetByIdAsync(id);

            current.Store.Name = request.Name;
            current.Store.Active = request.Active;

            return Ok(await repository.UpdateAsync(id, current.Store));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] string id)
    {
        try
        {
            await repository.DeleteAsync(id);
            return NoContent();
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await repository.GetAllAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int pageIndex = 0, int pageSize = 10, string filter = null)
    {
        return Ok(await repository.GetAsync(pageIndex, pageSize, filter));
    }
}