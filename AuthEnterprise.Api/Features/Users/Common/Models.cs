using System;

namespace AuthEnterprise.Api.Features.Users.Common;

public record UserModel(
    Ulid Id,
    string Username,
    string Email,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive);
