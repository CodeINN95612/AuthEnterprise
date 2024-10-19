using System;

using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Users;

public static class Endpoint
{
    public static WebApplication MapUsers(this WebApplication app)
    {
        app
            .MapGroup("/users")
            .WithTags("Users")
            .MapCreateUser()
            .MapGetUserById()
            .MapGetAllUsers()
            .MapDeleteUserById();
        return app;
    }
}
