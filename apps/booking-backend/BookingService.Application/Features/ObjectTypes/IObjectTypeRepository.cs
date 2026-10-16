using BookingService.Domain.ObjectTypes;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;

namespace BookingService.Application.Features.ObjectTypes
{
    public interface IObjectTypeRepository
    {
        Task<List<ObjectTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Result<Guid, Error>> AddAsync(ObjectType objectType);
    }
}
