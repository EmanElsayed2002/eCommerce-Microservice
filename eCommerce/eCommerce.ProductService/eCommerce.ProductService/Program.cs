using System.Text.Json.Serialization;

using eCommerce.API.Endpoints;
using eCommerce.API.Middleware;
using eCommerce.BusinessLogicLayer;
using eCommerce.BusinessLogicLayer.Validator;
using eCommerce.DataAccessLayer;

using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(option =>
{
    option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    option.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwaggerUI();
app.UseSwagger();
app.UseHttpsRedirection();
app.UserGlobalException();
app.UseAuthorization();

app.MapControllers();
app.MapProductAPIEndpoint();
app.Run();
