using System;
using Microsoft.Extensions.Options;
using LSL.Sentinet.ApiClient.DependencyInjection;
using LSL.Sentinet.ApiClient;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LSL.Sentinet.ApiClient.Facades;

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
        /// <param name="optionsConfigurator">
        /// A delegate to configure the Sentinet API options
        /// </param>
        /// <param name="httpClientBuilderConfigurator">
        /// A delegate to configure the Sentinet API <see cref="IHttpClientBuilder"><c>IHttpClientBuilder</c></see>
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddSentinetApiClient(
            this IServiceCollection source,
            Action<SentinetApiOptions> optionsConfigurator,
            Action<IHttpClientBuilder> httpClientBuilderConfigurator = null)
        {
            var httpClientBuilder = source.Configure(optionsConfigurator)
                .FluentTryAddTransient<SentinetApiMessageHandler>()
                .FluentTryAddSingleton<IFoldersFacade, FoldersFacade>()
                .AddHttpClient<ISentinetApiClient, SentinetApiClient>()
                .ConfigureHttpClient();

            httpClientBuilderConfigurator?.Invoke(httpClientBuilder);
            
            return source;
        }

        private static IHttpClientBuilder ConfigureHttpClient(this IHttpClientBuilder source)
        {
            return source.ConfigureHttpClient((services, client) =>
            {
                client.BaseAddress = new Uri(services.GetRequiredService<IOptions<SentinetApiOptions>>().Value.BaseUrl);
            })
            .AddHttpMessageHandler<SentinetApiMessageHandler>();            
        }
    }
}