using System.Security.Claims;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.Bookings;

internal static class BookingRules
{
    public const string AdministratorRoleName = "Administrator";

    public const string CancelledStatusName = "Отменено";

    public const string ConfirmedStatusName = "Подтверждено";

    public const string PendingStatusName = "Ожидает подтверждения";

    public static Result<Guid, Error> GetUserId(ClaimsPrincipal principal)
    {
        var userIdString = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId) || userId == Guid.Empty)
            return Error.Authentication("auth.invalid_token", "Недействительный токен");

        return userId;
    }
}
