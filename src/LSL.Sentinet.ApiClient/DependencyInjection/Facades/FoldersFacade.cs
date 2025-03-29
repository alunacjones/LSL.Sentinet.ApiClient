using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LSL.Sentinet.ApiClient.DependencyInjection.Facades.Models;

namespace LSL.Sentinet.ApiClient.DependencyInjection.Facades
{
    internal class FoldersFacade : IFoldersFacade
    {
        private readonly ISentinetApiClient _sentinetApiClient;

        public FoldersFacade(ISentinetApiClient sentinetApiClient)
        {
            _sentinetApiClient = sentinetApiClient;
        }

        public ISentinetApiClient Client => _sentinetApiClient;

        public async Task<GetFolderResult> GetFolderAsync(string fullPath, CancellationToken cancellationToken = default)
        {
            var result = (await fullPath.Split('/')
                .ToAsyncEnumerable()
                .AggregateAwaitAsync(
                    new 
                    { 
                        CurrentFolder = await GetFolderFromApi(null, string.Empty),
                        CurrentPath = string.Empty
                    }, 
                    async (folder, path) =>
                    {     
                        var currentPath = folder.CurrentPath + (folder.CurrentPath.Length == 0 ? path : $"/{path}");

                        return new 
                        {
                            CurrentFolder = await GetFolderFromApi(
                                folder.CurrentFolder, 
                                path, 
                                currentPath,
                                GetSubFolderId(folder.CurrentFolder, path)
                            ),
                            CurrentPath = currentPath
                        };
                    }))
                    .CurrentFolder;

            return new GetFolderResult(result, await Client.GetFolderAsync(result.SubTree.Id));

            int GetSubFolderId(FacadeFolderSubTree folder, string subPath) => 
                folder.SubTree.Folders.FirstOrDefault(f => f.Name == subPath)?.Id
                    ?? throw new ArgumentException($"Unknown folder '{subPath}' for full path of '{fullPath}'");

            async Task<FacadeFolderSubTree> GetFolderFromApi(FacadeFolderSubTree currentFolder, string path, string currentFullPath = "", int? id = null) => 
                (await Client.GetFolderSubtreeAsync(false, Entities.All, id, cancellationToken)
                    .ConfigureAwait(false))
                    .ToFacadeFolder(currentFullPath, currentFolder);
        }
    }

    internal static class FolderExtensions
    {
        internal static FacadeFolderSubTree ToFacadeFolder(this FolderSubtree source, string fullPath, FacadeFolderSubTree currentFolder) => 
            new FacadeFolderSubTree(currentFolder, fullPath, source);        
    }
}