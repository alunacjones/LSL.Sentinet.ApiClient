using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LSL.Sentinet.ApiClient.Facades.Models;

namespace LSL.Sentinet.ApiClient.Facades
{
    internal class FoldersFacade : Facade, IFoldersFacade
    {
        public FoldersFacade(ISentinetApiClient sentinetApiClient) : base(sentinetApiClient) { }

        public async Task<FacadeFolderSubTree> GetFolderAsync(string fullPath, CancellationToken cancellationToken = default)
        {
            return (await fullPath.Split('/')
                .ToAsyncEnumerable()
                .AggregateAwaitAsync(
                    new
                    {
                        CurrentFolder = await GetFolderFromApi(null, string.Empty).ConfigureAwait(false),
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
                            ).ConfigureAwait(false),
                            CurrentPath = currentPath
                        };
                    }).ConfigureAwait(false))
                    .CurrentFolder;

            int GetSubFolderId(FacadeFolderSubTree folder, string subPath) =>
                folder.SubTree.Folders.FirstOrDefault(f => f.Name.Equals(subPath, StringComparison.InvariantCultureIgnoreCase))?.Id
                    ?? throw new FolderNotFoundException(subPath, fullPath);

            async Task<FacadeFolderSubTree> GetFolderFromApi(FacadeFolderSubTree currentFolder, string path, string currentFullPath = "", int? id = null) =>
                (await Client.GetFolderSubtreeAsync(false, Entities.All, id, cancellationToken)
                    .ConfigureAwait(false))
                    .ToFacadeFolder(currentFullPath, currentFolder);
        }

        public async Task<FacadeFolderSubTree> TryGetFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetFolderAsync(folderPath, cancellationToken);
            }
            catch (FolderNotFoundException)
            {
                return null;
            }
        }
    }

    internal static class FolderExtensions
    {
        internal static FacadeFolderSubTree ToFacadeFolder(this FolderSubtree source, string fullPath, FacadeFolderSubTree currentFolder) =>
            new FacadeFolderSubTree(currentFolder, fullPath, source);
    }
}