using System.Threading;
using System.Threading.Tasks;
using LSL.Sentinet.ApiClient.Facades.Models;

namespace LSL.Sentinet.ApiClient.Facades
{
    /// <summary>
    /// A facade to the Sentinet API that provides
    /// easier to use methods for working with folders
    /// </summary>
    public interface IFoldersFacade : IFacade
    {
        /// <summary>
        /// Fetches a folder by its path
        /// </summary>
        /// <remarks>
        /// Path sections should be separated by a <c>/</c>
        /// e.g. <c>Consumers/MyService</c>
        /// </remarks>
        /// <param name="folderPath">The path to the folders</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FacadeFolderSubTree> GetFolderAsync(string folderPath, CancellationToken cancellationToken = default);
    }
}