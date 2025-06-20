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

        /// <summary>
        /// Tries to fetches a folder by its path
        /// </summary>
        /// <remarks>
        /// Path sections should be separated by a <c>/</c>
        /// e.g. <c>Consumers/MyService</c>.
        /// Returns <see langword="null" /> if not found
        /// </remarks>
        /// <param name="folderPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FacadeFolderSubTree> TryGetFolderAsync(string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a folder
        /// </summary>
        /// <remarks>
        /// Attempts to create all the required folders in the given folder path.
        /// If a folder already exists then nothing is done
        /// </remarks>
        /// <param name="folderPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FacadeFolderSubTree> CreateFolderAsync(string folderPath, CancellationToken cancellationToken = default); 
    }
}