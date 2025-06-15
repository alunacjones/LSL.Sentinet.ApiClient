namespace LSL.Sentinet.ApiClient.Facades
{
    /// <inheritdoc/>
    public abstract class Facade : IFacade
    {
        private readonly ISentinetApiClient _sentinetApiClient;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="sentinetApiClient"></param>
        protected internal Facade(ISentinetApiClient sentinetApiClient) => _sentinetApiClient = sentinetApiClient;

        /// <summary>
        /// The Sentinet API Client
        /// </summary>
        public ISentinetApiClient Client => _sentinetApiClient;
    }
}