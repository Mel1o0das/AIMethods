using BookingService.Domain.Bookings;
using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.Bookings
{
    public interface IBookingRepository
    {
        Task<List<BookingsDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> AddIfAvailableAsync(Booking booking, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default);
        Task<List<BookingsDto>> GetMyAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> ConfirmAsync(Guid id, BookingStatusId confirmedStatusId, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> CancelAsync(Guid id, Guid requesterUserId, bool isAdministrator, BookingStatusId cancelledStatusId, CancellationToken cancellationToken = default);
        Task<List<OccupiedSlotDto>> GetOccupiedSlotsAsync(Guid objectId, DateOnly date, BookingStatusId excludedStatusId, CancellationToken cancellationToken = default);
        Task<List<OccupiedSlotDto>> GetOccupiedSlotsForRangeAsync(Guid objectId, DateOnly from, DateOnly to, BookingStatusId excludedStatusId, CancellationToken cancellationToken = default);
    }
}
