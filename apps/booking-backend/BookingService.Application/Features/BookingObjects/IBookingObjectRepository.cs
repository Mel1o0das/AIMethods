using BookingService.Domain.BookingObjects;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.BookingObjects
{
    public interface IBookingObjectRepository
    {
        Task<List<BookingObjectsDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BookingObjectsPagedResult> GetPagedAsync(BookingObjectsPagedQuery query, CancellationToken cancellationToken = default);
        Task<Result<BookingObjectsDto, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> AddAsync(BookingObject bookingObject);
        Task<Result<Guid, Error>> UpdateAsync(Guid id, Name name, Description description, decimal price, int capacity, string? imageUrl, bool isActive, Guid? objectTypeId, CancellationToken cancellationToken = default);
        Task<Result<Guid, Error>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
