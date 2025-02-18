using System.Text;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.KeyValue.RangeScan;
using Couchbase.Query;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

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
    public async Task<IActionResult> AddAsync([FromBody] Store store)
    {
        store.CreatedAt = DateTimeOffset.UtcNow;
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

            return Ok(result.ContentAs<Store>());
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
            query.AppendLine($"SELECT * FROM {nameof(Store)} LIMIT {pageSize} OFFSET {pageIndex * pageSize}");

            IQueryResult<Store> queryResult = await _scope.QueryAsync<Store>(query.ToString());

            // StringBuilder query = new();
            // query.AppendLine($"SELECT * FROM `{nameof(Store)}`");

            // _collection.

            // IAsyncEnumerable<IScanResult> results = _collection.ScanAsync(
            //     new SamplingScan(limit: 100)
            // );

            // return Ok(result.ContentAs<Store>());
            return Ok();
        }
        catch (DocumentNotFoundException) {
            return NotFound();
        }
        /*
        using Couchbase;
using Couchbase.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Conecte-se ao cluster do Couchbase
        var cluster = await Cluster.ConnectAsync(
            "couchbase://localhost", // Endereço do cluster
            "username",              // Nome de usuário
            "password"               // Senha
        );

        // Acesse o bucket e o escopo/coleção
        var bucket = await cluster.BucketAsync("seu-bucket");
        var collection = bucket.DefaultCollection(); // Ou acesse uma coleção específica

        // Defina o número de documentos por página e a página atual
        int pageSize = 10;
        int pageNumber = 1; // Página 1, 2, 3, etc.
        int offset = (pageNumber - 1) * pageSize;

        // Construa a consulta N1QL com LIMIT e OFFSET
        var query = $@"
            SELECT *
            FROM `seu-bucket`
            WHERE tipo = 'exemplo'
            LIMIT $pageSize
            OFFSET $offset;
        ";

        // Execute a consulta
        var result = await cluster.QueryAsync<dynamic>(query, new Couchbase.Query.QueryOptions()
            .Parameter("pageSize", pageSize)
            .Parameter("offset", offset)
        );

        // Processe os resultados
        var documents = new List<dynamic>();
        await foreach (var row in result)
        {
            documents.Add(row);
        }

        // Exiba os documentos da página atual
        Console.WriteLine($"Documentos da página {pageNumber}:");
        foreach (var doc in documents)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(doc));
        }
    }
}
        */
    }
}