namespace LSL.Sentinet.ApiClient.Facades
{
    /// <summary>
    /// Base interface for all facades
    /// </summary>
    public interface IFacade
    {
        /// <summary>
        /// The Sentinet API client
        /// </summary>
        /// <value></value>
        ISentinetApiClient Client { get; }
    }
}