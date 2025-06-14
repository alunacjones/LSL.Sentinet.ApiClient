using System;
using System.Linq;
using LSL.Sentinet.ApiClient.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace LSL.Sentinet.ApiClient.Tests;

public class FoldersFacadeTests : BaseTest
{
    [Test]
    public async Task GivenAValidRequestForAFolder_ItShouldReturnTheExpectedResult()
    {
        try
        {
            // Arrange
            var provider = CreateServiceProvider(fakeService: true);
            var sut = provider.GetRequiredService<IFoldersFacade>();
            var folder = "Test/Folder";
            var folderResults = provider.StubFolderCalls(folder);

            // Act
            var response = await sut.GetFolderAsync(folder);

            // Assert
            using var assertionScope = new AssertionScope();

            response.SubTree.Should().Be(folderResults.ElementAt(2));
            response.FullPath.Should().Be("Test/Folder");
            response.Parent.SubTree.Should().Be(folderResults.ElementAt(1));
            response.Parent.FullPath.Should().Be("Test");
            response.Parent.Parent.SubTree.Should().Be(folderResults.ElementAt(0));
            response.Parent.Parent.FullPath.Should().Be("");

        }
        catch (Exception ex)
        {

            throw new ArgumentException("ad", ex);
        }
    }
}