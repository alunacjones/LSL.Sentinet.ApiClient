using System;
using LSL.Sentinet.ApiClient.Facades.Models;

namespace LSL.Sentinet.ApiClient.Facades
{
    /// <summary>
    /// Exception that is thrown when a folder is not found when
    /// using <see cref="IFoldersFacade"/> methods.
    /// </summary>
    public class FolderNotFoundException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="subPath"></param>
        /// <param name="fullPath"></param>
        /// <param name="folder"></param>
        public FolderNotFoundException(string subPath, string fullPath, FacadeFolderSubTree folder) : base($"Unknown folder '{subPath}' for full path of '{fullPath}'")
        {
            SubPath = subPath;
            FullPath = fullPath;
            Folder = folder;
        }

        /// <summary>
        /// The sub path that could not be located
        /// </summary>
        public string SubPath { get; }

        /// <summary>
        /// The full path
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// The folder that we managed to match up to
        /// </summary>
        public FacadeFolderSubTree Folder { get; }
    }
}