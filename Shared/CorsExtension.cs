namespace Shared;

public static class CorsExtension
{
    public static void AddAppCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("app",
                policy =>
                {
                    policy
                        .AllowAnyOrigin() // cambiar a algo origenes especificos
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                });
        });
    }
}