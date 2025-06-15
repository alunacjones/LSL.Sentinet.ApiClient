using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LSL.Sentinet.ApiClient.Facades.Models;

namespace LSL.Sentinet.ApiClient.Facades
{
    /// <inheritdoc/>
    public class FoldersFacade : Facade, IFoldersFacade
    {
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="sentinetApiClient"></param>
        public FoldersFacade(ISentinetApiClient sentinetApiClient) : base(sentinetApiClient) { }

        /// <inheritdoc/>
        public async Task<FacadeFolderSubTree> CreateFolderAsync(string folderPath, CancellationToken cancellationToken = default) =>
            await IterateFolder(
                folderPath,
                async (folder, path) =>
                {
                    var newFolder = await Client.CreateOrUpdateFolderWithResultAsync(new Folder
                    {
                        FolderId = folder.SubTree.Id,
                        Key = Guid.NewGuid(),
                        Name = path
                    }).ConfigureAwait(false);

                    return newFolder.Id;
                });

        /// <inheritdoc/>
        public async Task<FacadeFolderSubTree> GetFolderAsync(string fullPath, CancellationToken cancellationToken = default) =>
            await IterateFolder(fullPath, (folder, path) => throw new FolderNotFoundException(path, fullPath, folder));

        /// <inheritdoc/>
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

        internal async Task<FacadeFolderSubTree> IterateFolder(string fullPath, Func<FacadeFolderSubTree, string, Task<int>> action, CancellationToken cancellationToken = default)
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
                                    ?? await action(folder.CurrentFolder, path).ConfigureAwait(false)
                            ).ConfigureAwait(false),
                            CurrentPath = currentPath
                        };
                    }).ConfigureAwait(false))
                    .CurrentFolder;

            int? GetSubFolderId(FacadeFolderSubTree folder, string subPath) =>
                folder.SubTree.Folders.FirstOrDefault(f => f.Name.Equals(subPath, StringComparison.InvariantCultureIgnoreCase))?.Id;

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