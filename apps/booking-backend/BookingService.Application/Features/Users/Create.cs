using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Roles;
using BookingService.Application.Services;
using BookingService.Domain.Roles;
using BookingService.Domain.Shared;
using BookingService.Domain.Users;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Users;

public record CreateUserRequest(
    string Email,
    string Password,
    string LastName,
    string FirstName,
    string Patronymic,
    string Phone,
    Guid? RoleId = null);

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async Task<EndpointResult<Guid>> (
            [FromBody] CreateUserRequest request,
            [FromServices] CreateUserHandler handler) =>
                await handler.Handle(request));
    }
}

public sealed class CreateUserHandler
{
    private readonly ILogger<CreateUserHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRolesRepository _rolesRepository;

    public CreateUserHandler(ILogger<CreateUserHandler> logger, IUserRepository userRepository, IPasswordHasher passwordHasher, IRolesRepository rolesRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _rolesRepository = rolesRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return GeneralError.ValueIsInvalid("password");

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser.IsSuccess)
            return GeneralError.AlreadyExist();

        var passwordHash = PasswordHash.FromHash(_passwordHasher.Hash(request.Password));

        var fullNameResult = FullName.Create(request.LastName, request.FirstName, request.Patronymic);
        if (fullNameResult.IsFailure)
            return fullNameResult.Error;

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure)
            return phoneResult.Error;

        var roleResult = await _rolesRepository.GetByNameAsync("User");
        if (roleResult.IsFailure)
            return roleResult.Error;

        var user = User.Create(
            emailResult.Value,
            passwordHash,
            fullNameResult.Value,
            phoneResult.Value,
            roleResult.Value);

        var result = await _userRepository.AddAsync(user.Value);
        if (result.IsFailure)
            return result.Error;

        _logger.LogInformation("Created user {Id}", user.Value.Id.Value);

        return user.Value.Id.Value;
    }
}
