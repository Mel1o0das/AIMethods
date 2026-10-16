using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BookingService.Application.EndpointsSettings;

public class EndpointResult<TValue> : IResult
{
    private readonly IResult _result;

    public EndpointResult(Result<TValue, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _result.ExecuteAsync(httpContext);
    }

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);

    public static EndpointResult<TValue> ToEndpointResult(Result<TValue, Error> result) => new(result);
}

public class EndpointResult : IResult
{
    private readonly IResult _result;

    public EndpointResult(UnitResult<Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult()
            : new ErrorResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _result.ExecuteAsync(httpContext);
    }

    public static implicit operator EndpointResult(UnitResult<Error> result) => new(result);

    public static EndpointResult ToEndpointResult(UnitResult<Error> result) => new(result);
}
