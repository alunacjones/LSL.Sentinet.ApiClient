using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using LSL.Sentinet.ApiClient.DependencyInjection;
using LSL.Sentinet.ApiClient;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions
    /// </summary>
    public static class SentinetApiClientServiceCollectionExtensions
    {
        /// <summary>
        /// Registers everything needed to use <c>LSL.Sentinet.ApiClient</c> services
        /// </summary>
        /// <param name="source"></param>
        /// <param name="optionsConfigurator"></param>
        /// <returns></returns>
        public static IServiceCollection AddSentinetApiClient(this IServiceCollection source, Action<SentineApiOptions> optionsConfigurator)
        {
            source.Configure(optionsConfigurator)
                .AddTransient<SentinetApiMessageHandler>()
                .AddHttpClient<ISentinetApiClient, SentinetApiClient>()
                .ConfigureHttpClient();

            return source;
        }

        private static IHttpClientBuilder ConfigureHttpClient(this IHttpClientBuilder source)
        {
            return source.ConfigureHttpClient((services, client) =>
            {
                client.BaseAddress = new Uri(services.GetRequiredService<IOptions<SentineApiOptions>>().Value.BaseUrl);
            })
            .AddHttpMessageHandler<SentinetApiMessageHandler>();            
        }
    }
}