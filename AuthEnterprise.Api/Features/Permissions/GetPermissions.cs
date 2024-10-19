using System;

using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Permissions.Common;

using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AuthEnterprise.Api.Features.Permissions;

public static class GetPermissions
{
    public record GetPermissionsRequest : IRequest<ErrorOr<GetPermissionsResponse>>;
    public record GetPermissionsResponse(
        List<PermissionModel> Permissions);

    public static WebApplication MapGetPermissions(this WebApplication app)
    {
        app.MapGet("/permissions", async (ISender mediatr) =>
        {
            var response = await mediatr.Send(new GetPermissionsRequest());
            return response.Match(
                result => Results.Ok(result),
                errors => errors.ToProblemResult()
            );
        });

        return app;
    }

    public class GetPermissionsHandler : IRequestHandler<GetPermissionsRequest, ErrorOr<GetPermissionsResponse>>
    {
        private readonly AuthEnterpriseDbContext _db;

        public GetPermissionsHandler(AuthEnterpriseDbContext db)
        {
            _db = db;
        }

        public async Task<ErrorOr<GetPermissionsResponse>> Handle(GetPermissionsRequest request, CancellationToken cancellationToken)
        {
            var permissions = await _db.Permissions
                .Select(p => new PermissionModel(
                    p.Id,
                    p.Code,
                    p.Name,
                    p.Description,
                    p.Module,
                    p.Action
                ))
                .ToListAsync();
            return new GetPermissionsResponse(permissions);
        }
    }
}
