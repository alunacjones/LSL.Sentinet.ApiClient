using System;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests.TestHelpers.Api;

public static class MockHttpHelpers
{
    public static MockHttpMessageHandler CreateMockHttpMessageHandler()
    {
        var result = new MockHttpMessageHandler();
        
        result.Fallback.Respond(request => 
            throw new ArgumentException($"Unexpected HttpClient call: {request}. Have you forgotten to register a call request with MockHttpMessageHandler?"));

        return result;
    }

    public static void StubEndPointsAndAuth(this MockHttpMessageHandler source, bool authFails, bool isAlreadAuthorised)
    {
        var mockedRequest = new MockedRequest();

        source.AddBackendDefinition(mockedRequest);
        var isAuthorised = false;

        mockedRequest.Respond(r => 
        {
            if (r.RequestUri.LocalPath.EndsWith("/LogOn"))
            {
                isAuthorised = true;
                return new StringContent(authFails ? "false" : "true").ToHttpResponseMessage();
            }

            if (isAuthorised || isAlreadAuthorised)
            {
                return JsonContent.Create(new {}).ToHttpResponseMessage();
            }

            return new StringContent("no-care").ToHttpResponseMessage(HttpStatusCode.Unauthorized);
        });
    }

    internal static Task<HttpResponseMessage> ToHttpResponseMessage(this HttpContent source, HttpStatusCode httpStatusCode = HttpStatusCode.OK) => 
        Task.FromResult(new HttpResponseMessage(httpStatusCode)
        {
            Content = source
        });
}
