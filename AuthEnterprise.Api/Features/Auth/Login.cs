using AuthEnterprise.Api.Abstractions.Generators;
using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
namespace AuthEnterprise.Api.Features.Auth;

public static class Login
{
    public record LoginRequest(string Username, string Password)
        : IRequest<ErrorOr<LoginResponse>>;

    public record LoginResponse(string Token);

    public static RouteGroupBuilder MapLogin(this RouteGroupBuilder app)
    {
        app.MapPost("/login", async (LoginRequest request, ISender mediatr) =>
        {
            var response = await mediatr.Send(request);
            return response.Match(
                result => Results.Ok(result),
                errors => errors.ToProblemResult()
            );
        })
        .WithName("Login")
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Login",
            Description = "Login to the system using your username and password."
        });
        return app;
    }

    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(l => l.Username)
                .NotEmpty()
                .NotNull();

            RuleFor(l => l.Password)
                .NotEmpty()
                .NotNull();
        }
    }

    public class LoginHandler : IRequestHandler<LoginRequest, ErrorOr<LoginResponse>>
    {
        private readonly IValidator<LoginRequest> _validator;
        private readonly AuthEnterpriseDbContext _db;
        private readonly IJwtGenerator _jwtGenerator;

        public LoginHandler(
            AuthEnterpriseDbContext db,
            IValidator<LoginRequest> validator,
            IJwtGenerator jwtGenerator)
        {
            _db = db;
            _validator = validator;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<ErrorOr<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => Error.Validation(e.ErrorMessage)).ToList();
                return ErrorOr<LoginResponse>.From(errors);
            }

            var user = await _db.Users.Where(u => u.Username == request.Username).FirstOrDefaultAsync();
            if (user is null)
            {
                return Error.Conflict("Invalid Credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Error.Conflict("Invalid Credentials");
            }

            var rolePermissions = await _db.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(p => p.Permission)
                .Select(r => new
                {
                    Role = r.Name,
                    Permission = r.RolePermissions.Select(rp => rp.Permission.Code)
                })
                .ToListAsync();

            var token = _jwtGenerator.GenerateToken(
                user.Id,
                user.Username,
                user.Email,
                rolePermissions.Select(rp => rp.Role).ToList(),
                [.. rolePermissions.SelectMany(rp => rp.Permission).ToList(), "users.delete"]);

            return new LoginResponse(
                Token: token
            );
        }
    }
}