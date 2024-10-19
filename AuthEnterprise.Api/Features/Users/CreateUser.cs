using System.Data;

using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Users.Common;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace AuthEnterprise.Api.Features.Users;

public static class CreateUser
{
    public record CreateUserRequest(
        string Username,
        string Email,
        string Password
    ) : IRequest<ErrorOr<CreateUserResponse>>;

    public record CreateUserResponse(UserModel User);

    public static RouteGroupBuilder MapCreateUser(this RouteGroupBuilder app)
    {
        app.MapPost("/", async (ISender mediatr, CreateUserRequest request) =>
        {
            var response = await mediatr.Send(request);
            return response.Match(
                result => Results.Created($"/users/{result.User.Id}", result),
                errors => errors.ToProblemResult()
            );
        })
        .WithName("CreateUser")
        // .Produces<CreateUserResponse>(StatusCodes.Status201Created)
        // .Produces(StatusCodes.Status400BadRequest)
        // .Produces(StatusCodes.Status409Conflict)
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Create User",
            Description = "Create a new user.",
            Responses = new OpenApiResponses
            {
                ["201"] = new OpenApiResponse { Description = "Created successfully" },
                ["400"] = new OpenApiResponse { Description = "Bad request - Invalid input" },
                ["401"] = new OpenApiResponse { Description = "Unauthorized - Is not authenticated" },
                ["403"] = new OpenApiResponse { Description = "Forbidden - Does not have permission" },
                ["409"] = new OpenApiResponse { Description = "Conflict - User already exists" }
            }
        });

        return app;
    }

    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .MaximumLength(50)
                .MinimumLength(3)
                .Matches("^[a-zA-Z0-9]*$")
                    .WithMessage("Username must be alphanumeric.")
                .Must(username => !username.Contains(" "))
                    .WithMessage("Username must not contain spaces");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .MaximumLength(50)
                .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$")
                    .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number.");
        }
    }

    public class CreateUserHandler : IRequestHandler<CreateUserRequest, ErrorOr<CreateUserResponse>>
    {
        public readonly static Ulid DefaultRoleId = Ulid.Parse("01JA96MKSH6S7X9DADG086X2QY");

        private readonly IValidator<CreateUserRequest> _validator;
        private readonly AuthEnterpriseDbContext _db;

        public CreateUserHandler(IValidator<CreateUserRequest> validator, AuthEnterpriseDbContext db)
        {
            _validator = validator;
            _db = db;
        }

        public async Task<ErrorOr<CreateUserResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => Error.Validation(e.ErrorMessage)).ToList();
                return ErrorOr<CreateUserResponse>.From(errors);
            }

            var existingUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser is not null)
            {
                return Error.Conflict("User already exists.");
            }

            var hashedPassword = HashPassword(request.Password);

            User newUser = new()
            {
                Id = Ulid.NewUlid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Users.Add(newUser);

            var role = await _db.Roles.FindAsync(DefaultRoleId);
            if (role is null)
            {
                return Error.NotFound("Default user roles does not exist.");
            }

            var newUserRole = new UserRole
            {
                RoleId = role.Id,
                UserId = newUser.Id,
                AssinedAt = DateTime.UtcNow,
                Role = role,
                User = newUser
            };

            _db.UserRoles.Add(newUserRole);

            await _db.SaveChangesAsync();

            return new CreateUserResponse(new(
                newUser.Id,
                newUser.Username,
                newUser.Email,
                newUser.CreatedAt,
                newUser.UpdatedAt,
                newUser.IsActive
            ));
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
