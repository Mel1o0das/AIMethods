using BookingService.Application.EndpointsSettings;
using BookingService.Application.Features.Roles;
using BookingService.Application.Features.Users;
using BookingService.Application.Services;
using BookingService.Domain.Shared;
using BookingService.Domain.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, Guid UserId, string Email);

public class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async Task<EndpointResult<LoginResponse>> (
            [FromBody] LoginRequest request,
            [FromServices] LoginHandler handler) =>
                await handler.Handle(request));
    }
}

public sealed class LoginHandler
{
    private readonly ILogger<LoginHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(
        ILogger<LoginHandler> logger,
        IUserRepository userRepository,
        IRolesRepository rolesRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _rolesRepository = rolesRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResponse, Error>> Handle(LoginRequest request)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        var userResult = await _userRepository.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
            return Error.Authentication("auth.invalid_credentials", "Неверный email или пароль");

        var user = userResult.Value;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash.Value))
            return Error.Authentication("auth.invalid_credentials", "Неверный email или пароль");

        var roles = await _rolesRepository.GetAllAsync();
        var userRole = roles.Find(r => r.id == user.Role.Value);
        var roleName = userRole?.Name.Value ?? string.Empty;

        var token = _jwtTokenService.GenerateToken(user, roleName);

        _logger.LogInformation("User {Id} logged in", user.Id.Value);

        return new LoginResponse(token, user.Id.Value, user.Email.Value);
    }
}
