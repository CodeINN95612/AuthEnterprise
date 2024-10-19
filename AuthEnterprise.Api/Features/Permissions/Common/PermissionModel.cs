using System;

namespace AuthEnterprise.Api.Features.Permissions.Common;

public record PermissionModel(
    Ulid Id,
    string Code,
    string Name,
    string Description,
    string Module,
    string Action
);
