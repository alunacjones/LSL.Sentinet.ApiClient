namespace LSL.Sentinet.ApiClient.Facades.Models
{
    /// <summary>
    /// A Sentinet folder sub tree composite
    /// that provides extra information
    /// for easier traversal
    /// </summary>
    public class FacadeFolderSubTree
    {
        internal FacadeFolderSubTree(FacadeFolderSubTree parent, string fullPath, FolderSubtree subTree)
        {
            Parent = parent;
            FullPath = fullPath;
            SubTree = subTree;
        }

        /// <summary>
        /// The parent folder
        /// </summary>
        /// <value></value>
        public FacadeFolderSubTree Parent { get; }

        /// <summary>
        /// The full path to this folder
        /// </summary>
        /// <value></value>
        public string FullPath { get; }

        /// <summary>
        /// The folder from the Sentinet API
        /// </summary>
        /// <value></value>
        public FolderSubtree SubTree { get; }
    }
}