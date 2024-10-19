
using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Users.Common;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Users;

public static class GetAllUsers
{
    public record GetAllUsersRequest : IRequest<ErrorOr<List<UserModel>>>;
    public record GetAllUsersResponse(List<UserModel> Users);

    public static RouteGroupBuilder MapGetAllUsers(this RouteGroupBuilder app)
    {
        app.MapGet("/", async (ISender mediatr) =>
        {
            var response = await mediatr.Send(new GetAllUsersRequest());
            return response.Match(
                result => Results.Ok(result),
                errors => errors.ToProblemResult()
            );
        })
        .WithName("GetAllUsers")
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Get All Users",
            Description = "Get all users in the system."
        });

        return app;
    }

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, ErrorOr<List<UserModel>>>
    {
        private readonly AuthEnterpriseDbContext _db;

        public GetAllUsersHandler(AuthEnterpriseDbContext db)
        {
            _db = db;
        }

        public async Task<ErrorOr<List<UserModel>>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
        {
            var users = await _db.Users
                .Select(u => new UserModel(
                    u.Id,
                    u.Username,
                    u.Email,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.IsActive
                ))
                .ToListAsync();

            return users;
        }
    }
}
