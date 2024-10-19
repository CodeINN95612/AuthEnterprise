using AuthEnterprise.Api.Abstractions.Generators;
using AuthEnterprise.Api.Common.Constants;
using AuthEnterprise.Api.Database;
using AuthEnterprise.Api.Extensions;
using AuthEnterprise.Api.Features.Auth;
using AuthEnterprise.Api.Features.Auth.Common;
using AuthEnterprise.Api.Features.Users;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AuthEnterprise.Api.Auth;
using AuthEnterprise.Api.Features.Permissions;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthEnterprise API", Version = "v1" });
      c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
          Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey,
          Scheme = "Bearer"
      });

      c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
  });
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddSingleton<IJwtGenerator, JwtGenerator>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddDbContext<AuthEnterpriseDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(DbConstants.AuthDBConnectionString));
});

builder.Services.AddAuth(builder.Configuration);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "AuthEnterprise API"));
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("AuthEnterprise API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithPreferredScheme("Bearer");
    });

}

app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuth();
app.MapUsers();

app.MapGetPermissions();
app.MapCreatePermission();

app.Run();
