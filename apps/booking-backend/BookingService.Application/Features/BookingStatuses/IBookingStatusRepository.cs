using BookingService.Domain.BookingStatuses;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.BookingStatuses
{
    public interface IBookingStatusRepository
    {
        Task<List<BookingStatusesDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> AddAsync(BookingStatus bookingStatus);
        Task<Result<BookingStatusId, Error>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
