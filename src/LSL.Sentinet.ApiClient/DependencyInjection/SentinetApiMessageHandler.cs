using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace LSL.Sentinet.ApiClient.DependencyInjection
{
    /// <summary>
    /// An options-friendly Sentinet API message handler
    /// </summary>
    internal class SentinetApiMessageHandler : DelegatingHandler
    {
        private readonly IOptionsMonitor<SentinetApiOptions> _optionsMonitor;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="optionsMonitor"></param>
        public SentinetApiMessageHandler(IOptionsMonitor<SentinetApiOptions> optionsMonitor) => _optionsMonitor = optionsMonitor;

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response =  await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
            
            var currentValue = _optionsMonitor.CurrentValue;

            return await request.AuthoriseAndResend(base.SendAsync, currentValue.Username, currentValue.Password, cancellationToken);
        }
    }
}