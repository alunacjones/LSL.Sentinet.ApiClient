using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests.TestHelpers.Api;

public static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddFakeHttpMessageHandler(
        this IServiceCollection source,
        MockHttpMessageHandler mockHttpMessageHandler)
    {
        source.AddScoped(_ => mockHttpMessageHandler);

        source.ConfigureAll<HttpClientFactoryOptions>(options => options
            .HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = mockHttpMessageHandler)
        );

        return source;
    }

    public static IEnumerable<FolderSubtree> StubFolderCalls(this IServiceProvider source, string pathToStub)
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        var apiClient = source.GetRequiredService<ISentinetApiClient>();
        var segments = pathToStub.Split('/').Prepend(string.Empty).ToArray();

        return segments.Aggregate(new { result = new List<FolderSubtree>(), index = 0 }, (agg, i) =>
        {
            var folder = Create(i, agg.index, segments.Length - 1 == agg.index ? null : segments[agg.index + 1])
                .With(f => f.Id = agg.index + 1);

            if (string.IsNullOrEmpty(i))
            {
                apiClient.GetFolderSubtreeAsync(false, Entities.All)
                    .Returns(folder);
            }
            else
            {
                apiClient.GetFolderSubtreeAsync(false, Entities.All, folder.Id)
                    .Returns(folder);
            }

            agg.result.Add(folder);

            return new
            {
                agg.result,
                index = agg.index + 1
            };
        })
        .result;

        FolderSubtree Create(string name, int index, string nextSegment = null)
        {
            var result = fixture
                .Create<FolderSubtree>()
                .With(f =>
                {
                    f.Name = name;
                    f.Id = index + 1;
                });
            
            result.Folders = [fixture.Create<FolderSubtree>().With(f => {
                f.Name = nextSegment;
                f.Id = index + 2;
            })];

            return result;
        }
    }

    public static T With<T>(this T source, Action<T> configurator)
    {
        configurator(source);
        return source;
    }
}