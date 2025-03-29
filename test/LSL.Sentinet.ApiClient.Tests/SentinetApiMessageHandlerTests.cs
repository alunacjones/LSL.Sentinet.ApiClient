using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests;

public class SentinetApiMessageHandlerTests
{
    [Test]
    public async Task SentinetApiMessageHandler_GivenInvalidAuthOptions_ItShouldThrowTheExpectedException()
    {
        var client = CreateClient(authFails: true, isAlreadAuthorised: false);

        var act = async () => await client.GetFolderAsync(0);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task SentinetApiMessageHandler_GivenValidAuthOptions_ItShouldReturnTheData()
    {
        var client = CreateClient(authFails: false, isAlreadAuthorised: false);

        var response = await client.GetFolderAsync(0);

        response.Should().BeOfType<Folder>();
    }

    [Test]
    public async Task SentinetApiMessageHandler_GivenValidAuthOptionsAndWeAreAlreadyAuthorised_ItShouldReturnTheData()
    {
        var client = CreateClient(authFails: false, isAlreadAuthorised: true);

        var response = await client.GetFolderAsync(0);
        response.Should().BeOfType<Folder>();
    }

    private static ISentinetApiClient CreateClient(bool authFails, bool isAlreadAuthorised)
    {
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

        mockHttpHandler.StubEndPointsAndAuth(authFails, isAlreadAuthorised);

        mockHttpHandler.When(HttpMethod.Get, "http://nowhere.com/RepositoryService.svc/LogOn?userName=username&password=password")
            .Respond(new StringContent(authFails ? "false" : "true"));

        return services.GetRequiredService<ISentinetApiClient>();
    }    
}