using MealBridge.Api.Contracts;
using MealBridge.Api.Contracts.Donations;
using MealBridge.Application.Donations.Commands;
using MealBridge.Application.Donations.Queries;
using MealBridge.Application.Donations.UseCases;
using MealBridge.Domain.Donations;
using MealBridge.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MealBridge.Api.Controllers;

[ApiController]
[Route("api/donations")]
public sealed class DonationsController(
    CreateDonation createDonation,
    ListDonations listDonations,
    GetDonation getDonation,
    ClaimDonation claimDonation,
    ChangeDonationStatus changeDonationStatus) : ControllerBase
{
    private const string FoodCategoryRequiredMessage =
        "La categoría de alimento es obligatoria.";
    private const string DonationUnitRequiredMessage =
        "La unidad de donación es obligatoria.";
    private const string StatusRequiredMessage =
        "El estado es obligatorio.";
    private const string InvalidStatusFilterMessage =
        "El filtro de estado no es válido.";
    private const string InvalidFoodCategoryFilterMessage =
        "El filtro de categoría de alimento no es válido.";
    private const string InvalidDonationIdMessage =
        "El identificador del lote no es válido.";
    private const string ClientDefinedStatusMessage =
        "El estado no puede definirse al crear un lote.";

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDonationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DefinesStatus)
        {
            throw new InvalidArgumentException(
                ClientDefinedStatusMessage);
        }

        var foodCategory = request.FoodCategory
            ?? throw new InvalidArgumentException(
                FoodCategoryRequiredMessage);
        var unit = request.Unit
            ?? throw new InvalidArgumentException(
                DonationUnitRequiredMessage);

        var lot = await createDonation.ExecuteAsync(
            new CreateDonationCommand(
                request.BusinessName!,
                request.Title!,
                request.Description,
                foodCategory,
                request.Quantity,
                unit,
                request.PickupAddress!,
                request.AvailableFrom,
                request.AvailableUntil),
            cancellationToken);
        var response = DonationResponse.FromDomain(lot);

        return CreatedAtAction(
            nameof(GetById),
            new { id = lot.Id },
            ApiEnvelope<DonationResponse>.Success(response));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] string? foodCategory,
        CancellationToken cancellationToken)
    {
        var filters = new DonationFilters(
            ParseStatusFilter(status),
            ParseFoodCategoryFilter(foodCategory));
        var lots = await listDonations.ExecuteAsync(
            filters,
            cancellationToken);
        var response = lots
            .Select(DonationResponse.FromDomain)
            .ToArray();

        return Ok(
            ApiEnvelope<IReadOnlyList<DonationResponse>>.Success(response));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var lot = await getDonation.ExecuteAsync(
            ParseId(id),
            cancellationToken);
        return Ok(
            ApiEnvelope<DonationResponse>.Success(
                DonationResponse.FromDomain(lot)));
    }

    [HttpPost("{id}/claim")]
    public async Task<IActionResult> Claim(
        string id,
        ClaimDonationRequest request,
        CancellationToken cancellationToken)
    {
        var lot = await claimDonation.ExecuteAsync(
            ParseId(id),
            request.CoordinatorName!,
            cancellationToken);
        return Ok(
            ApiEnvelope<DonationResponse>.Success(
                DonationResponse.FromDomain(lot)));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(
        string id,
        ChangeDonationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var targetStatus = request.Status
            ?? throw new InvalidArgumentException(StatusRequiredMessage);
        var lot = await changeDonationStatus.ExecuteAsync(
            ParseId(id),
            targetStatus,
            cancellationToken);
        return Ok(
            ApiEnvelope<DonationResponse>.Success(
                DonationResponse.FromDomain(lot)));
    }

    private static DonationStatus? ParseStatusFilter(string? value) =>
        value switch
        {
            null => null,
            "available" => DonationStatus.Available,
            "claimed" => DonationStatus.Claimed,
            "picked_up" => DonationStatus.PickedUp,
            "cancelled" => DonationStatus.Cancelled,
            "expired" => DonationStatus.Expired,
            _ => throw new InvalidArgumentException(
                InvalidStatusFilterMessage)
        };

    private static FoodCategory? ParseFoodCategoryFilter(string? value) =>
        value switch
        {
            null => null,
            "bakery" => FoodCategory.Bakery,
            "produce" => FoodCategory.Produce,
            "dairy" => FoodCategory.Dairy,
            "prepared" => FoodCategory.Prepared,
            "other" => FoodCategory.Other,
            _ => throw new InvalidArgumentException(
                InvalidFoodCategoryFilterMessage)
        };

    private static Guid ParseId(string value) =>
        Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidArgumentException(
                InvalidDonationIdMessage);
}
