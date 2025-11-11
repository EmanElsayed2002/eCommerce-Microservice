using System.Text.Json.Serialization;

using eCommerce.BusinessLogicLayer;
using eCommerce.BusinessLogicLayer.HttpClientt;
using eCommerce.BusinessLogicLayer.Policies;
using eCommerce.BusinessLogicLayer.Validator;
using eCommerce.DataAccessLayer;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

ConfigureHttpClientsWithPolicies(builder.Services, builder.Configuration);

void ConfigureHttpClientsWithPolicies(IServiceCollection services, IConfiguration configuration)
{
    services.AddHttpClient<ProductsMicroserviceClient>((serviceProvider, client) =>
    {
        var gatewayUrl = Environment.GetEnvironmentVariable("GatewayBaseUrl") 
            ?? configuration["GatewayBaseUrl"] 
            ?? "http://apigateway:8080";
        client.BaseAddress = new Uri(gatewayUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var policies = serviceProvider.GetRequiredService<IProductsMicroservicePolicies>();
        
      
        return Policy.WrapAsync(
            policies.GetBulkheadIsolationPolicy(),
            policies.GetFallbackPolicy()
        );
    });

    services.AddHttpClient<UsersMicroserviceClient>((serviceProvider, client) =>
    {
        var gatewayUrl = Environment.GetEnvironmentVariable("GatewayBaseUrl") 
            ?? configuration["GatewayBaseUrl"] 
            ?? "http://apigateway:8080";
        client.BaseAddress = new Uri(gatewayUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
        
        return policies.GetCombinedPolicy();
    });
}
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
