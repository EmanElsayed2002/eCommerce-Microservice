using eCommerce.API.Middleware;
using eCommerce.Core.Repository;
using eCommerce.Core.Service;
using eCommerce.Core.Validator;
using eCommerce.Infrastructure.DbContext;
using eCommerce.Infrastructure.Repository;

using FluentValidation;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options=> options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
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

app.MapControllers();

app.Run();
