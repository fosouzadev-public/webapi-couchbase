using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCouchbase(builder.Configuration.GetSection("Couchbase"));
builder.Services.AddCouchbaseBucket<INamedBucketProvider>(builder.Configuration["Couchbase:Bucket"]);
builder.Services.AddSingleton<IScope>(provider =>
{
    INamedBucketProvider bucketProvider = provider.GetRequiredService<INamedBucketProvider>();
    IBucket bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();

    return bucket.DefaultScopeAsync().GetAwaiter().GetResult();
});

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

app.Lifetime.ApplicationStopped.Register(async () =>
{
    await app.Services.GetRequiredService<ICouchbaseLifetimeService>().CloseAsync().ConfigureAwait(false);
});

app.Run();