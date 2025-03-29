using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests.TestHelpers.Api;

public static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddFakeHttpMessageHandler(
        this IServiceCollection source,
        MockHttpMessageHandler mockHttpMessageHandler)
    {
        source.AddScoped(_  => mockHttpMessageHandler);

        source.ConfigureAll<HttpClientFactoryOptions>(options => options
            .HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = mockHttpMessageHandler)
        );
        
        return source;
    }    
}