namespace LSL.Sentinet.ApiClient.Facades
{
    internal abstract class Facade : IFacade
    {
        private readonly ISentinetApiClient _sentinetApiClient;

        internal Facade(ISentinetApiClient sentinetApiClient)
        {
            _sentinetApiClient = sentinetApiClient;
        }

        public ISentinetApiClient Client => _sentinetApiClient;
    }
}