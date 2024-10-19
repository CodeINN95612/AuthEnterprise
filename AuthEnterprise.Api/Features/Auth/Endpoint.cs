using System;

using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Auth;

public static class Endpoint
{
    public static WebApplication MapAuth(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group
            .MapLogin();
        return app;
    }
}
