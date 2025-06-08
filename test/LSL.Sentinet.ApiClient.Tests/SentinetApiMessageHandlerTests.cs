using System;
using System.Linq;
using DotNetEnv;
using LSL.Sentinet.ApiClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests;

public class SentinetApiMessageHandlerTests
{
    [Test]
    public async Task SentinetApiMessageHandler_GivenInvalidAuthOptions_ItShouldThrowTheExpectedException()
    {
        var client = CreateClient(authFails: true, isAlreadyAuthorised: false);

        var act = async () => await client.GetFolderAsync(0);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task SentinetApiMessageHandler_GivenValidAuthOptions_ItShouldReturnTheData()
    {
        var client = CreateClient(authFails: false, isAlreadyAuthorised: false);

        var response = await client.GetFolderAsync(0);

        response.Should().BeOfType<Folder>();
    }

    [Test]
    public async Task SentinetApiMessageHandler_GivenValidAuthOptionsAndWeAreAlreadyAuthorised_ItShouldReturnTheData()
    {
        var client = CreateClient(authFails: false, isAlreadyAuthorised: true);

        var response = await client.GetFolderAsync(0);
        response.Should().BeOfType<Folder>();
    }

    [Test]
    public async Task Connect()
    {
        var sut = CreateServiceProvider().GetRequiredService<IFoldersFacade>();
        var result = await sut.GetFolderAsync("Microservices/Payments");

        foreach (var service in result.SubTree.Services)
        {
            var svc = await sut.Client.GetServiceAsync(service.Id);

            foreach (var version in svc.ServiceVersions)
            {
                var v = await sut.Client.GetServiceVersionAsync(version.ServiceVersionId);
                var dep = await sut.Client.GetDependenciesAsync(true, 1, 1, [new LocalIdentifier
                {
                    EntityType = EntityType.ServiceVersion,
                    Id = v.Id
                }]);
            }
        }
    }

    private static ISentinetApiClient CreateClient(bool authFails, bool isAlreadyAuthorised)
    {
        InitialiseEnv();
        var mockHttpHandler = CreateMockHttpMessageHandler();

        var services = new ServiceCollection()
            .AddFakeHttpMessageHandler(mockHttpHandler)
            .AddSentinetApiClient(c => 
            {
                c.BaseUrl = "http://nowhere.com/RepositoryService.svc/";
                c.Username = "username";
                c.Password = "password";
            })
            .BuildServiceProvider();

        mockHttpHandler.StubEndPointsAndAuth(authFails, isAlreadyAuthorised);

        mockHttpHandler.When(HttpMethod.Get, "http://nowhere.com/RepositoryService.svc/LogOn?userName=username&password=password")
            .Respond(new StringContent(authFails ? "false" : "true"));

        return services.GetRequiredService<ISentinetApiClient>();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        InitialiseEnv();

        return new ServiceCollection()
            .AddSentinetApiClient(c => 
            {
                c.BaseUrl = "https://alstest/Sentinet/RepositoryService.svc/";
                c.Username = GetUsername();
                c.Password = GetPassword();
            },
            httpClientBuilderConfigurator: h => h.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            }))
            .BuildServiceProvider();
    }

    private static void InitialiseEnv() => Env.TraversePath().Load();

    internal static string GetUsername() => Environment.GetEnvironmentVariable("SENTINET_USERNAME");
    internal static string GetPassword() => Environment.GetEnvironmentVariable("SENTINET_PASSWORD");
}