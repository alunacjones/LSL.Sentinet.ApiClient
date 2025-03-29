using System.Threading;
using System.Threading.Tasks;
using LSL.Sentinet.ApiClient.DependencyInjection.Facades.Models;

namespace LSL.Sentinet.ApiClient.DependencyInjection
{
    /// <summary>
    /// A facade to the Sentinet API that provides
    /// easier to use methods work with folders
    /// </summary>
    public interface IFoldersFacade
    {
        /// <summary>
        /// The Sentinet API client
        /// </summary>
        /// <value></value>
        ISentinetApiClient Client { get; }

        /// <summary>
        /// Fetches a folder sub tree by its path
        /// </summary>
        /// <remarks>
        /// Paths should be separated by a <c>/</c>
        /// e.g. <c>Consumers/MyService</c>
        /// </remarks>
        /// <param name="folderPath">The path to the folders</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FacadeFolderSubTree> GetFolderAsync(string folderPath, CancellationToken cancellationToken = default);
    }
}