using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;
using Play.Catalog.Service.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceSettings = builder.Configuration
    .GetSection(nameof(ServiceSettings))
    .Get<ServiceSettings>();

//// Configure MongoDB serialization
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

//// Configure dependency injection
builder.Services.AddSingleton(serviceProvider =>
{
    var mongoDbSettings = builder.Configuration
        .GetSection(nameof(MongoDbSettings))
        .Get<MongoDbSettings>();
    var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
    return mongoClient.GetDatabase(serviceSettings?.ServiceName);
});

builder.Services.AddSingleton<IRepository<Item>>(serviceProvider =>
{
    // get the registered MongoDB service
    var database = serviceProvider.GetService<IMongoDatabase>();
    return new MongoRepository<Item>(database!, "items");
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
    // transform /App-Entities to /app-entities
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
