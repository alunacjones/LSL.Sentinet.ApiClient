namespace LSL.Sentinet.ApiClient.DependencyInjection.Facades.Models
{
    /// <summary>
    /// The result of fetching a folder
    /// </summary>
    public readonly struct GetFolderResult
    {
        internal GetFolderResult(FacadeFolderSubTree facadeFolderSubTree, Folder folder)
        {
            FacadeFolderSubTree = facadeFolderSubTree;
            Folder = folder;
        }

        /// <summary>
        /// The folder sub tree
        /// </summary>
        /// <value></value>
        public FacadeFolderSubTree FacadeFolderSubTree { get; }

        /// <summary>
        /// The full folder entity
        /// </summary>
        /// <value></value>
        public Folder Folder { get; }

        /// <summary>
        /// Deconstructs <see cref="GetFolderResult"><c>GetFolderResult</c></see> to a <see cref="ApiClient.Folder"><c>Folder</c></see>
        /// </summary>
        /// <param name="folder"></param>
        public void Deconstruct(out Folder folder) => folder = Folder;
    }
}