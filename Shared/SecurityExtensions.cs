using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Shared;

public static class SecurityExtensions
{

    public static IServiceCollection AddSecurityExtensions(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKeyResolver = (token, securityToken, keyIdentifier, validationParameters) =>
                    {
                        var client = new HttpClient();
                        var response = client.GetAsync("https://accessmanagercloudnative1.b2clogin.com/accessmanagercloudnative1.onmicrosoft.com/B2C_1_singinsignup_rukayun/discovery/v2.0/keys").Result;
                        var json = response.Content.ReadAsStringAsync().Result;
                        var jwks = new JsonWebKeySet(json);
                        return jwks.Keys;
                    },
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

        services.AddAuthorization(opt =>
        {
            opt.AddPolicy(Policies.Admin, p =>
            {
                p.RequireClaim("extension_Roles", Policies.Admin);
            });
            opt.AddPolicy(Policies.User, p =>
            {
                p.RequireClaim("extension_Roles", Policies.User);
            });
            opt.AddPolicy(Policies.Colaborator, p =>
            {
                p.RequireClaim("extension_Roles", Policies.Colaborator);
            });
            opt.AddPolicy(Policies.SuperAdmin, p =>
            {
                p.RequireClaim("extension_Roles", Policies.SuperAdmin);
            });
            opt.AddPolicy(Policies.AdminOrColaborator, P =>
            {
                P.RequireClaim("extension_Roles", Policies.Admin, Policies.Colaborator);
            });
            opt.AddPolicy(Policies.SuperAdminOrAdmin, P =>
            {
                P.RequireClaim("extension_Roles", Policies.Admin, Policies.SuperAdmin);
            });
        });
        return services;
    }

    
    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
    }
    
    public static string GetSurname(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
    }
    
    public static string GetUsername(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == "emails")?.Value ?? "";
    }
    public static string GetUserRole(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == "extension_Roles")?.Value ?? "";
    }

    public static bool ISSuperAdmin(this ClaimsPrincipal principal)
    {
        return principal.GetUserRole() == Policies.SuperAdmin; 
    }
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.GetUserRole() == Policies.Admin; 
    }
    public static bool IsUser(this ClaimsPrincipal principal)
    {
        return principal.GetUserRole() == Policies.User; 
    }
    public static bool IsColaborator(this ClaimsPrincipal principal)
    {
        return principal.GetUserRole() == Policies.Colaborator; 
    }
    
}

public static class Policies
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string User = "USER";
    public const string Colaborator = "COLABORATOR";
    
    public const string AdminOrColaborator = "ADMIN_OR_COLABORATOR";
    public const string SuperAdminOrAdmin = "SUPER_ADMIN_OR_ADMIN";
}


