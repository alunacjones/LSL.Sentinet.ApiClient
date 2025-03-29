namespace LSL.Sentinet.ApiClient.DependencyInjection
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