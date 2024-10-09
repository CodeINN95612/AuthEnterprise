using ErrorOr;

using FluentValidation;

using MediatR;
namespace AuthEnterprise.Api.Features.Auth;

public static class Login
{
    public record LoginRequest(string Username, string Password) 
        : IRequest<ErrorOr<LoginResponse>>;

    public record LoginResponse(string Token);

    public static WebApplication MapLogin(this WebApplication app)
    {
        app.MapPost("/auth/login", async (LoginRequest request, ISender mediatr) => {
            var response = await mediatr.Send(request);
            return response.Match(
                result => Results.Ok(result),
                error => Results.BadRequest(error)
            );
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
        public class User
        {
            public required string Username {get; set;}
            public required string Password {get; set;}
        }

        private static readonly List<User> Users = [];

        public async Task<ErrorOr<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var user = Users.Where(u => u.Username == request.Username).FirstOrDefault();
            if (user is null )
            {
                return Error.Conflict("Invalid Credentials");
            }

            if (user.Password != request.Password)
            {
                return Error.Conflict("Invalid Credentials");
            }

            return new LoginResponse(
                Token: "sdjfklsdjflsdf"
            );
        }
    }
}