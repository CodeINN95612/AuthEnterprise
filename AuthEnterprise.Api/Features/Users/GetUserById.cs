
using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Users.Common;

using ErrorOr;

using MediatR;

using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Users;

public static class GetUserById
{
    public record GetUserByIdRequest(Ulid Id) : IRequest<ErrorOr<UserModel>>;
    public record GetUserByIdResponse(UserModel User);

    public static RouteGroupBuilder MapGetUserById(this RouteGroupBuilder app)
    {
        app.MapGet("/{id}", async (ISender mediatr, Ulid id) =>
        {
            var response = await mediatr.Send(new GetUserByIdRequest(id));
            return response.Match(
                result => Results.Ok(result),
                errors => errors.ToProblemResult()
            );
        })
        .WithName("GetUserById")
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {

            Summary = "Get User By ID",
            Description = "Get a user by their ID."
        }); ;

        return app;
    }

    public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, ErrorOr<UserModel>>
    {
        private readonly AuthEnterpriseDbContext _db;

        public GetUserByIdHandler(AuthEnterpriseDbContext db)
        {
            _db = db;
        }

        public async Task<ErrorOr<UserModel>> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(request.Id);
            if (user is null)
            {
                return Error.NotFound($"User with ID {request.Id} not found.");
            }
            return new UserModel(
                user.Id,
                user.Username,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt,
                user.IsActive);
        }
    }
}
