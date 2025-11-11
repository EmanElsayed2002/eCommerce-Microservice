using System.Text.Json.Serialization;

using eCommerce.API.Endpoints;
using eCommerce.API.Middleware;
using eCommerce.BusinessLogicLayer;
using eCommerce.BusinessLogicLayer.Validator;
using eCommerce.DataAccessLayer;

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(option=> 
{
    option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    option.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
// Configure JSON options for minimal APIs (MapPost, MapPut, etc.)
builder.Services.ConfigureHttpJsonOptions(options => 
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDataAccessLayer(builder.Configuration).AddBusinessLogicLayer();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AddProductValidator>();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UserGlobalException();
app.UseAuthorization();

// Apply pending EF Core migrations at startup to ensure tables exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<eCommerce.DataAccessLayer.MyDbContext.AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();
app.MapProductAPIEndpoint();
app.Run();
