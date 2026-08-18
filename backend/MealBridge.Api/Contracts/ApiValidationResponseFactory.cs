using Microsoft.AspNetCore.Mvc;

namespace MealBridge.Api.Contracts;

public static class ApiValidationResponseFactory
{
    private const string InvalidRequestMessage =
        "La solicitud contiene datos no válidos.";

    public static IActionResult Create(ActionContext _) =>
        new BadRequestObjectResult(
            ApiEnvelope<object>.Failure(InvalidRequestMessage));
}
