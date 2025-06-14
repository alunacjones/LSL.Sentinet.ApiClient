using System;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests;

public abstract class BaseTest
{
    public static void InitialiseEnv() => Env.TraversePath().Load();

    public static IServiceProvider CreateServiceProvider(bool authFails = false, bool isAlreadyAuthorised = false, bool fakeService = false)
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
            .With(s =>
            {
                if (fakeService)
                {
                    s.AddSingleton(Substitute.For<ISentinetApiClient>());
                }                
            })
            .BuildServiceProvider();

        mockHttpHandler.StubEndPointsAndAuth(authFails, isAlreadyAuthorised);

        mockHttpHandler.When(HttpMethod.Get, "http://nowhere.com/RepositoryService.svc/LogOn?userName=username&password=password")
            .Respond(new StringContent(authFails ? "false" : "true"));

        return services;
    }
}