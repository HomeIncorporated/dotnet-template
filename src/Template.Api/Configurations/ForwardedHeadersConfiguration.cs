using Microsoft.AspNetCore.HttpOverrides;

namespace Template.Api.Configurations;

public static class ForwardedHeadersConfiguration
{
    public static IApplicationBuilder UseForwardedHeadersConfiguration(this IApplicationBuilder app)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        app.UseForwardedHeaders(options);

        return app;
    }
}
