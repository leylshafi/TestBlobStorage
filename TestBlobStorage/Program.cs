using Microsoft.EntityFrameworkCore;
using TestBlobStorage;
using TestBlobStorage.Data;
using TestBlobStorage.Models;
using TestBlobStorage.Services;
using TestBlobStorage.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddSwagger();
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
var cosmos = new CosmosConfig();
builder.Configuration.GetSection("Cosmos").Bind(cosmos);

builder.Services.AddDbContext<CosmosDbContext>(op => op.UseCosmos(cosmos.Uri, cosmos.Key, cosmos.DatabaseName));
builder.Services.AddStorageManaganer(builder.Configuration);
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));


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
