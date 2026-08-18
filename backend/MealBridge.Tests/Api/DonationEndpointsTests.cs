using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MealBridge.Api.Middleware;
using MealBridge.Domain.Donations;
using MealBridge.Infrastructure.Persistence;
using MealBridge.Tests.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace MealBridge.Tests.Api;

public sealed class DonationEndpointsTests(
    DonationApiFactory factory) :
    IClassFixture<DonationApiFactory>,
    IAsyncLifetime
{
    private const string MalformedJson = "{\"businessName\":";
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task PostDonations_WhenValid_Returns201EnvelopeAndCamelCase()
    {
        var response = await client.PostAsJsonAsync(
            "/api/donations",
            CreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = await ReadBody(response);
        AssertSuccessEnvelope(body.RootElement);
        var data = body.RootElement.GetProperty("data");
        Assert.True(data.TryGetProperty("businessName", out _));
        Assert.False(data.TryGetProperty("BusinessName", out _));
        Assert.Equal("bakery", data.GetProperty("foodCategory").GetString());
        Assert.Equal("loaves", data.GetProperty("unit").GetString());
        Assert.Equal("available", data.GetProperty("status").GetString());

        var invalidResponse = await client.PostAsJsonAsync(
            "/api/donations",
            CreateRequest(quantity: 0));
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        using var invalidBody = await ReadBody(invalidResponse);
        AssertFailureEnvelope(invalidBody.RootElement);
    }

    [Fact]
    public async Task PostDonations_WhenStatusIsProvided_Returns400Envelope()
    {
        var request = new
        {
            businessName = "Panadería El Nogal",
            title = "Baguettes del día anterior",
            description = "Sirven todavía para tostadas y crotones.",
            foodCategory = "bakery",
            quantity = 24,
            unit = "loaves",
            pickupAddress = "Calle 79 #11-45, Bogotá",
            availableFrom = "2026-08-18T16:00:00Z",
            availableUntil = "2026-08-18T20:00:00Z",
            status = "claimed"
        };

        var response = await client.PostAsJsonAsync(
            "/api/donations",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = await ReadBody(response);
        AssertFailureEnvelope(body.RootElement);
    }

    [Fact]
    public async Task PostDonations_WhenJsonIsMalformed_Returns400Envelope()
    {
        using var content = new StringContent(
            MalformedJson,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/donations", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = await ReadBody(response);
        AssertFailureEnvelope(body.RootElement);
    }

    [Fact]
    public async Task GetDonations_WhenRequested_Returns200Envelope()
    {
        var response = await client.GetAsync(
            "/api/donations?status=available&foodCategory=bakery");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await ReadBody(response);
        AssertSuccessEnvelope(body.RootElement);
        Assert.Equal(
            JsonValueKind.Array,
            body.RootElement.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task GetDonations_WhenFilterIsUnknown_Returns400Envelope()
    {
        var invalidStatusResponse = await client.GetAsync(
            "/api/donations?status=unknown");
        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidStatusResponse.StatusCode);
        using var invalidStatusBody =
            await ReadBody(invalidStatusResponse);
        AssertFailureEnvelope(invalidStatusBody.RootElement);

        var invalidCategoryResponse = await client.GetAsync(
            "/api/donations?foodCategory=unknown");
        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidCategoryResponse.StatusCode);
        using var invalidCategoryBody =
            await ReadBody(invalidCategoryResponse);
        AssertFailureEnvelope(invalidCategoryBody.RootElement);
    }

    [Fact]
    public async Task GetDonation_WhenUnknownOrMalformed_ReturnsEnvelopeError()
    {
        var missingResponse = await client.GetAsync(
            $"/api/donations/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        using var missingBody = await ReadBody(missingResponse);
        AssertFailureEnvelope(missingBody.RootElement);

        var malformedResponse = await client.GetAsync(
            "/api/donations/no-es-un-uuid");
        Assert.Equal(HttpStatusCode.BadRequest, malformedResponse.StatusCode);
        using var malformedBody = await ReadBody(malformedResponse);
        AssertFailureEnvelope(malformedBody.RootElement);
    }

    [Fact]
    public async Task PostClaim_WhenRepeated_Returns200Then409WithoutOverwrite()
    {
        var id = await CreateDonation();
        var firstResponse = await client.PostAsJsonAsync(
            $"/api/donations/{id}/claim",
            new { coordinatorName = "Banco de Alimentos Uno" });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            $"/api/donations/{id}/claim",
            new { coordinatorName = "Banco de Alimentos Dos" });
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        using var secondBody = await ReadBody(secondResponse);
        AssertFailureEnvelope(secondBody.RootElement);

        var getResponse = await client.GetAsync($"/api/donations/{id}");
        using var getBody = await ReadBody(getResponse);
        Assert.Equal(
            "Banco de Alimentos Uno",
            getBody.RootElement
                .GetProperty("data")
                .GetProperty("claimedBy")
                .GetString());
    }

    [Fact]
    public async Task PatchStatus_WhenLegalThenIllegal_Returns200Then409()
    {
        var id = await CreateDonation();
        await client.PostAsJsonAsync(
            $"/api/donations/{id}/claim",
            new { coordinatorName = "Banco de Alimentos Uno" });

        var successResponse = await client.PatchAsJsonAsync(
            $"/api/donations/{id}/status",
            new { status = "picked_up" });
        Assert.Equal(HttpStatusCode.OK, successResponse.StatusCode);
        using var successBody = await ReadBody(successResponse);
        Assert.Equal(
            "picked_up",
            successBody.RootElement
                .GetProperty("data")
                .GetProperty("status")
                .GetString());

        var conflictResponse = await client.PatchAsJsonAsync(
            $"/api/donations/{id}/status",
            new { status = "cancelled" });
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        using var conflictBody = await ReadBody(conflictResponse);
        AssertFailureEnvelope(conflictBody.RootElement);
    }

    [Fact]
    public async Task PatchStatus_WhenTargetIsClaimed_Returns409Envelope()
    {
        var id = await CreateDonation();

        var response = await client.PatchAsJsonAsync(
            $"/api/donations/{id}/status",
            new { status = "claimed" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = await ReadBody(response);
        AssertFailureEnvelope(body.RootElement);
    }

    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<MealBridgeDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<MealBridgeDbContext>();
        await context.Database.EnsureDeletedAsync();
        client.Dispose();
    }

    private async Task<Guid> CreateDonation()
    {
        var lot = DonationLot.Create(
            "Panadería El Nogal",
            "Baguettes del día anterior",
            "Sirven todavía para tostadas y crotones.",
            FoodCategory.Bakery,
            24,
            DonationUnit.Loaves,
            "Calle 79 #11-45, Bogotá",
            new DateTimeOffset(2026, 8, 18, 16, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 18, 20, 0, 0, TimeSpan.Zero));
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<MealBridgeDbContext>();
        context.DonationLots.Add(lot);
        await context.SaveChangesAsync();
        return lot.Id;
    }

    private static object CreateRequest(int quantity = 24) => new
    {
        businessName = "Panadería El Nogal",
        title = "Baguettes del día anterior",
        description = "Sirven todavía para tostadas y crotones.",
        foodCategory = "bakery",
        quantity,
        unit = "loaves",
        pickupAddress = "Calle 79 #11-45, Bogotá",
        availableFrom = "2026-08-18T16:00:00Z",
        availableUntil = "2026-08-18T20:00:00Z"
    };

    private static async Task<JsonDocument> ReadBody(
        HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    private static void AssertSuccessEnvelope(JsonElement root)
    {
        Assert.True(root.GetProperty("succeeded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("error").ValueKind);
    }

    private static void AssertFailureEnvelope(JsonElement root)
    {
        Assert.False(root.GetProperty("succeeded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("error").GetString()));
    }
}

public sealed class DonationApiFactory :
    WebApplicationFactory<GlobalExceptionHandler>
{
    private const string TestDatabaseName = "mealbridge_api_tests";
    private readonly string connectionString;

    public DonationApiFactory()
    {
        var configured =
            TestEnvironment.GetMealBridgeConnectionString();
        connectionString = new NpgsqlConnectionStringBuilder(configured)
        {
            Database = TestDatabaseName
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MealBridge"] = connectionString
                }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<MealBridgeDbContext>();
            services.RemoveAll<DbContextOptions<MealBridgeDbContext>>();
            services.AddDbContext<MealBridgeDbContext>(
                options => options.UseNpgsql(connectionString));
        });
    }
}
