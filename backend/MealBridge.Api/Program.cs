using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using MealBridge.Api.Configuration;
using MealBridge.Api.Contracts;
using MealBridge.Api.Middleware;
using MealBridge.Application.Donations.UseCases;
using MealBridge.Infrastructure;
using Microsoft.AspNetCore.Mvc;

const string MealBridgeConnectionName = "MealBridge";
const string RequiredValueMessage = "El valor solicitado es obligatorio.";
Env.NoClobber().TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
var corsSettings = builder.Configuration
    .GetRequiredSection(CorsSettings.SectionName)
    .Get<CorsSettings>()!;

builder.Services
    .AddControllers(options =>
        options.ModelBindingMessageProvider
            .SetValueMustNotBeNullAccessor(
                _ => RequiredValueMessage))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(
                JsonNamingPolicy.SnakeCaseLower,
                allowIntegerValues: false));
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory =
        ApiValidationResponseFactory.Create);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString(MealBridgeConnectionName));
builder.Services.AddScoped<CreateDonation>();
builder.Services.AddScoped<ListDonations>();
builder.Services.AddScoped<GetDonation>();
builder.Services.AddScoped<ClaimDonation>();
builder.Services.AddScoped<ChangeDonationStatus>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSettings.PolicyName, policy =>
        policy.WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyHeader()
            .WithMethods(corsSettings.AllowedMethods));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseCors(CorsSettings.PolicyName);
app.MapControllers();
app.MapGet("/health", () =>
    Results.Ok(ApiEnvelope<object>.Success(new { status = "healthy" })));

app.Run();
