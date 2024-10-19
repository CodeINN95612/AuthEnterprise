
using AuthEnterprise.Api.Auth;
using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Users;

public static class DeleteUserById
{
    public record DeleteUserByIdRequest(Ulid Id) : IRequest<ErrorOr<Deleted>>;

    public static RouteGroupBuilder MapDeleteUserById(this RouteGroupBuilder app)
    {
        app.MapDelete("/{id}", async (Ulid id, ISender mediatr) =>
        {
            var response = await mediatr.Send(new DeleteUserByIdRequest(id));
            return response.Match(
                _ => Results.Ok(),
                errors => errors.ToProblemResult()
            );
        })
        .RequirePermission("users.delete")
        .WithName("DeleteUserById")
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Delete User by Id",
            Description = "Delete a user by their id."
        });

        return app;
    }

    public class DeleteUserByIdHandler : IRequestHandler<DeleteUserByIdRequest, ErrorOr<Deleted>>
    {
        private readonly AuthEnterpriseDbContext _db;

        public DeleteUserByIdHandler(AuthEnterpriseDbContext db)
        {
            _db = db;
        }

        public async Task<ErrorOr<Deleted>> Handle(DeleteUserByIdRequest request, CancellationToken cancellationToken)
        {
            var existingUser = await _db.Users.FindAsync(request.Id);
            if (existingUser is null)
            {
                return Error.NotFound($"User with id {request.Id} not found");
            }

            _db.Users.Remove(existingUser);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Deleted;
        }
    }
}