using System;
using System.Linq;
using DotNetEnv;
using LSL.Sentinet.ApiClient.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace LSL.Sentinet.ApiClient.Tests;

public class SentinetApiMessageHandlerTests : BaseTest
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
    [Ignore("dev only")]
    public async Task Connect()
    {
        Env.TraversePath().Load();
        var sut = CreateServiceProvider().GetRequiredService<IFoldersFacade>();
        var result = await sut.GetFolderAsync(Environment.GetEnvironmentVariable("SENTINET_TEST_PATH"));

        foreach (var service in result.SubTree.Services.Skip(1))
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
            //{Id: 1545, EntityType: 5}
        }
    }

    private static ISentinetApiClient CreateClient(bool authFails, bool isAlreadyAuthorised) =>
        CreateServiceProvider(authFails, isAlreadyAuthorised).GetRequiredService<ISentinetApiClient>();

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

    internal static string GetUsername() => Environment.GetEnvironmentVariable("SENTINET_USERNAME");
    internal static string GetPassword() => Environment.GetEnvironmentVariable("SENTINET_PASSWORD");
}