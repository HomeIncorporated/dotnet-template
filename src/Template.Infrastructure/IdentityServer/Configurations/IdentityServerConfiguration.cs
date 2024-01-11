using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Domain.Common.Models;
using Template.Domain.Identity.Entites;
using Template.Infrastructure.IdentityServer.Services;

namespace Template.Infrastructure.IdentityServer.Configurations;

public static class IdentityServerConfiguration
{
    public static IServiceCollection ConfigureIdentityServer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var migrationAssembly = typeof(IdentityServerConfiguration).Assembly.FullName;
        var settings = configuration.GetSection("AppConfig").Get<AppConfig>();

        services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.Authentication.CookieLifetime = TimeSpan.FromDays(30);
                options.Authentication.CookieSlidingExpiration = true;
                options.IssuerUri = settings.IdentityServerConfig.IssuerUri;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = context =>
                    context.UseSqlServer(
                        connectionString,
                        sql => sql.MigrationsAssembly(migrationAssembly)
                    );
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = context =>
                    context.UseSqlServer(
                        connectionString,
                        sql => sql.MigrationsAssembly(migrationAssembly)
                    );

                options.EnableTokenCleanup = true;
            })
            .AddProfileService<ProfileService>()
            .AddAspNetIdentity<User>();

        return services;
    }
}
