using eCommerce.BusinessLogicLayer;
using eCommerce.BusinessLogicLayer.HttpClientt;
using eCommerce.BusinessLogicLayer.Policies;
using eCommerce.BusinessLogicLayer.Validator;
using eCommerce.DataAccessLayer;

using FluentValidation;
using FluentValidation.AspNetCore;

using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

ConfigureHttpClientsWithPolicies(builder.Services, builder.Configuration);
void ConfigureHttpClientsWithPolicies(IServiceCollection services, IConfiguration configuration)
{
    services.AddHttpClient<ProductsMicroserviceClient>((serviceProvider, client) =>
    {
        var gatewayUrl = Environment.GetEnvironmentVariable("GatewayBaseUrl")
            ?? configuration["GatewayBaseUrl"]
            ?? "https://localhost:7252/";
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
            ?? "https://localhost:7155/";
        client.BaseAddress = new Uri(gatewayUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();

        return policies.GetCombinedPolicy();
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
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
