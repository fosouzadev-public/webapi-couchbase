using Couchbase;
using Couchbase.KeyValue;

var builder = WebApplication.CreateBuilder(args);

AddCouchbase(builder.Services, builder.Configuration).GetAwaiter().GetResult();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

async Task AddCouchbase(IServiceCollection services, IConfiguration configuration)
{
    ClusterOptions clusterOptions = new()
    {
        UserName = configuration["Couchbase:Username"],
        Password = configuration["Couchbase:Password"]
    };
    clusterOptions.ApplyProfile("wan-development");
    
    ICluster cluster = await Cluster.ConnectAsync(configuration["Couchbase:ConnectionString"], clusterOptions);
    IBucket bucket = await cluster.BucketAsync(configuration["Couchbase:Bucket"]);
    IScope scope = await bucket.ScopeAsync(configuration["Couchbase:Scope"]);

    services.AddSingleton(scope);
}