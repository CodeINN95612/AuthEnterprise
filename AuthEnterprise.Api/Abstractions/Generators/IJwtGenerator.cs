using System;

namespace AuthEnterprise.Api.Abstractions.Generators;

public interface IJwtGenerator
{
    string GenerateToken(Ulid userId, string username, string email, List<string> roles, List<string> permissions);
}
