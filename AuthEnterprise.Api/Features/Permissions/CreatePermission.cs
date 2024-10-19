using System;

using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Permissions.Common;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AuthEnterprise.Api.Features.Permissions;

public static class CreatePermission
{

    public record CreatePermissionRequest(
        string Code,
        string Name,
        string Description) : IRequest<ErrorOr<CreatePermissionResponse>>;
    public record CreatePermissionResponse(PermissionModel permission);

    public static WebApplication MapCreatePermission(this WebApplication app)
    {
        app.MapPost("/permissions", async (CreatePermissionRequest request, ISender mediatr) =>
        {
            var response = await mediatr.Send(request);
            return response.Match(
                result => Results.CreatedAtRoute($"/permissions/{result.permission.Id}", result),
                errors => errors.ToProblemResult()
            );
        });

        return app;
    }

    public class CreatePermissionValidator : AbstractValidator<CreatePermissionRequest>
    {
        public CreatePermissionValidator()
        {
            RuleFor(x => x.Code)
                .MinimumLength(3)
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9.]*$")
                    .WithMessage("'Code' must be alphanumeric.")
                .Must(code => code.Count(c => c == '.') == 1)
                    .WithMessage("'Code' must contain one and only one dot")
                .Must(code =>
                {
                    var parts = code.Split('.');
                    return parts.Length == 2 && parts.All(p => p.Length > 0);
                }).WithMessage("Code must contain one module and one action Ex: 'module.action'");

            RuleFor(x => x.Name)
                .MinimumLength(3)
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MinimumLength(3)
                .MaximumLength(300);
        }
    }

    public class CreatePermissionHandler : IRequestHandler<CreatePermissionRequest, ErrorOr<CreatePermissionResponse>>
    {
        private readonly IValidator<CreatePermissionRequest> _validator;
        private readonly AuthEnterpriseDbContext _db;

        public CreatePermissionHandler(IValidator<CreatePermissionRequest> validator, AuthEnterpriseDbContext db)
        {
            _validator = validator;
            _db = db;
        }

        public async Task<ErrorOr<CreatePermissionResponse>> Handle(CreatePermissionRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => Error.Validation(e.ErrorMessage)).ToList();
                return ErrorOr<CreatePermissionResponse>.From(errors);
            }

            var existingPermission = await _db.Permissions.FirstOrDefaultAsync(p => p.Code == request.Code);

            if (existingPermission is not null)
            {
                return Error.Conflict($"Permission with code {request.Code} already exists");
            }

            var permission = Permission.Create(request.Code, request.Name, request.Description);

            _db.Permissions.Add(permission);

            await _db.SaveChangesAsync(cancellationToken);

            return new CreatePermissionResponse(new(
                permission.Id,
                permission.Code,
                permission.Name,
                permission.Description,
                permission.Module,
                permission.Action));
        }
    }
}


/*

POST /roles => crear rol

POSt /roles/{id}/permissions => agregar permiso a rol

POST /users => crear usuario

POST /user/{id}/roles => agregar rol a usuario

Pepito:
- Default
    - default.default
- Manager
    - users.create
    - users.read
    - users.update
    - users.delete
- Manager Assistant
    - users.read
    - users.update
- Accounting
    - accounting.create

*/
