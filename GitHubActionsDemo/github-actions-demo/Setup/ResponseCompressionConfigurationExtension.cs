using System.IO.Compression;

using Microsoft.AspNetCore.ResponseCompression;

namespace github_actions_demo.Setup;
public static class ResponseCompressionConfigurationExtension
{
    private static readonly string[] mimeTypes = new[]{
                     // Default,
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    "font/woff2",
                    "image/x-icon",
                    "image/png",
                    "image/svg",
                    "ap­pli­ca­tion/xhtml+xml",
                    "ap­pli­ca­tion/atom+xml",
                    "image/svg+xml"
                };

    public static IServiceCollection AddResponseCompressions(
       this IServiceCollection services)
    {
        //response compression
        services.AddResponseCompression(options =>
            {
                options.MimeTypes =
                ResponseCompressionDefaults.MimeTypes.Concat(mimeTypes);
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = true;
            });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }
}
