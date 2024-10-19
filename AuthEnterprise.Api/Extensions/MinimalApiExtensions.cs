using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Extensions;

public static class MinimalApiExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission)
    {
        builder.RequireAuthorization($"Permission:{permission}");
        builder
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        return builder;
    }


    public static RouteHandlerBuilder ConfigureEndpoint<TResponse>(
        this RouteHandlerBuilder builder,
        string summary,
        string description,
        int successStatusCode,
        bool requiresAuthorization = false,
        bool canReturnBadRequest = true,
        bool canReturnConflict = false)
    {
        builder
            .WithName(summary.Replace(" ", ""))
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = summary,
                Description = description
            });

        // Add success response
        Type responseType = typeof(TResponse);
        if (responseType == typeof(void))
        {
            builder.Produces(successStatusCode);
        }
        else
        {
            builder.Produces(successStatusCode, responseType);
        }

        // Define ProblemDetails with errors extension
        var problemDetailsWithErrors = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = "string" },
                ["title"] = new OpenApiSchema { Type = "string" },
                ["status"] = new OpenApiSchema { Type = "integer" },
                ["detail"] = new OpenApiSchema { Type = "string" },
                ["instance"] = new OpenApiSchema { Type = "string" },
                ["errors"] = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" }
                    }
                }
            }
        };

        // Add error responses
        var responses = new Dictionary<int, string>
        {
            { StatusCodes.Status500InternalServerError, "Internal server error" }
        };

        if (canReturnBadRequest)
        {
            responses.Add(StatusCodes.Status400BadRequest, "Bad request");
        }

        if (requiresAuthorization)
        {
            responses.Add(StatusCodes.Status401Unauthorized, "Unauthorized");
            responses.Add(StatusCodes.Status403Forbidden, "Forbidden");
        }

        if (canReturnConflict)
        {
            responses.Add(StatusCodes.Status409Conflict, "Conflict");
        }

        foreach (var response in responses)
        {
            builder.ProducesProblem(response.Key);
        }

        // Customize OpenAPI operation to include the ProblemDetails with errors schema
        builder.WithOpenApi(operation =>
        {
            foreach (var response in responses)
            {
                if (response.Key == StatusCodes.Status400BadRequest ||
                    response.Key == StatusCodes.Status409Conflict ||
                    response.Key == StatusCodes.Status500InternalServerError)
                {
                    operation.Responses[response.Key.ToString()].Content["application/problem+json"].Schema = problemDetailsWithErrors;
                }
            }
            return operation;
        });

        return builder;
    }
}
