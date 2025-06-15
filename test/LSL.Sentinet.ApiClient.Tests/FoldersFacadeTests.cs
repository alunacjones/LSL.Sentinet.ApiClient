using System.Linq;
using LSL.Sentinet.ApiClient.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace LSL.Sentinet.ApiClient.Tests;

public class FoldersFacadeTests : BaseTest
{
    [Test]
    public async Task GetFolderAsync_GivenAValidRequestForAFolder_ItShouldReturnTheExpectedResult()
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

    [Test]
    public async Task GetFolderAsync_GivenAnInvalidRequestForAFolder_ItShouldThrowTheExpectedException()
    {
        // Arrange
        var provider = CreateServiceProvider(fakeService: true, builder: c => { });
        var sut = provider.GetRequiredService<IFoldersFacade>();
        var folder = "Test/Folder";
        var folderResults = provider.StubFolderCalls(folder);
        folderResults.ElementAt(0).Folders.ElementAt(0).Name = "Wrong";

        // Act & Assert
        var action = async () => await sut.GetFolderAsync(folder);
        await action.Should().ThrowExactlyAsync<FolderNotFoundException>()
            .WithMessage("Unknown folder 'Test' for full path of 'Test/Folder'")
            .Where(x => x.SubPath == "Test" && x.FullPath == "Test/Folder" && x.Folder != null);
    }    
    
    [Test]
    public async Task TryGetFolderAsync_GivenAValidRequestForAFolder_ItShouldReturnTheExpectedResult()
    {
        // Arrange
        var provider = CreateServiceProvider(fakeService: true);
        var sut = provider.GetRequiredService<IFoldersFacade>();
        var folder = "Test/Folder";
        var folderResults = provider.StubFolderCalls(folder);

        // Act
        var response = await sut.TryGetFolderAsync(folder);

        // Assert
        using var assertionScope = new AssertionScope();
        
        response.SubTree.Should().Be(folderResults.ElementAt(2));
        response.FullPath.Should().Be("Test/Folder");
        response.Parent.SubTree.Should().Be(folderResults.ElementAt(1));
        response.Parent.FullPath.Should().Be("Test");
        response.Parent.Parent.SubTree.Should().Be(folderResults.ElementAt(0));
        response.Parent.Parent.FullPath.Should().Be("");
    }

    [Test]
    public async Task TryGetFolderAsync_GivenAnInvalidRequestForAFolder_ItShouldThrowTheExpectedException()
    {
        // Arrange
        var provider = CreateServiceProvider(fakeService: true, builder: c => { });
        var sut = provider.GetRequiredService<IFoldersFacade>();
        var folder = "Test/Folder";
        var folderResults = provider.StubFolderCalls(folder);
        folderResults.ElementAt(0).Folders.ElementAt(0).Name = "Wrong";

        // Act 
        var response = await sut.TryGetFolderAsync(folder);

        // Assert
        response.Should().BeNull();
    }     
}