using System.Xml;

using ErrorOr;

using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthEnterprise.Api.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblemResult(this List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return TypedResults.Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return errors.ToValidationProblemResult();
        }

        return errors[0].ToProblemResult();
    }

    private static ProblemHttpResult ToProblemResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        var extensions = new Dictionary<string, object?>()
        {
            { "errors", new List<string>{error.Code}}
        };

        return TypedResults.Problem(statusCode: statusCode, title: error.Description, extensions: extensions);
    }

    private static ProblemHttpResult ToValidationProblemResult(this List<Error> errors)
    {
        var validationErrors = new Dictionary<string, object?>()
        {
            { "errors", errors.Select(error => error.Code).ToArray() }
        };

        return TypedResults.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "One or more validation errors occurred.",
            extensions: validationErrors);
    }
}
