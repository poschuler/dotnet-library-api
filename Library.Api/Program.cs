using FluentValidation;
using FluentValidation.Results;
using Library.Api;
using Library.Api.Auth;
using Library.Api.Data;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// var builder = WebApplication.CreateBuilder(new WebApplicationOptions
// {
//     Args = args,
//     WebRootPath = "./wwwroot",
//     EnvironmentName = "Development",
//     ApplicationName = "Library.Api"
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.IncludeFields = true;
});

//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString")!));

builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseEndpoints<Program>();

//DB init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();

