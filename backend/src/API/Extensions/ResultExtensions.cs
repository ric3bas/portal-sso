using Portal.Domain.Base;

namespace Portal.API.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Converte um Result&lt;T&gt; em IResult para Minimal APIs
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.Success)
        {
            return Results.Ok(result.Data);
        }

        return result.Type switch
        {
            ResultType.NotFound => Results.NotFound(result.Problem),
            ResultType.Validation => Results.BadRequest(result.Problem),
            ResultType.Business => Results.BadRequest(result.Problem),
            _ => Results.BadRequest(result.Problem)
        };
    }

    /// <summary>
    /// Converte um SimpleResult em IResult para Minimal APIs
    /// </summary>
    public static IResult ToHttpResult(this SimpleResult result)
    {
        if (result.Success)
        {
            return Results.Ok();
        }

        return result.Type switch
        {
            ResultType.NotFound => Results.NotFound(result.Problem),
            ResultType.Validation => Results.BadRequest(result.Problem),
            ResultType.Business => Results.BadRequest(result.Problem),
            _ => Results.BadRequest(result.Problem)
        };
    }

    /// <summary>
    /// Converte um Result&lt;T&gt; em Created result para operações de criação
    /// </summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, string? location = null)
    {
        if (result.Success)
        {
            return string.IsNullOrEmpty(location) 
                ? Results.Created(string.Empty, result.Data)
                : Results.Created(location, result.Data);
        }

        return result.Type switch
        {
            ResultType.NotFound => Results.NotFound(result.Problem),
            ResultType.Validation => Results.BadRequest(result.Problem),
            ResultType.Business => Results.BadRequest(result.Problem),
            _ => Results.BadRequest(result.Problem)
        };
    }
}
