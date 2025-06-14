using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LSL.Sentinet.ApiClient
{
    /// <summary>
    /// A message handler to be used in .NET 451 consumers
    /// consumers
    /// </summary>
    public class LegacySentinetApiMessageHandler : DelegatingHandler
    {
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username">The username to use to log into the Sentinet API</param>
        /// <param name="password">The password to use to log into the Sentinet API</param>
        public LegacySentinetApiMessageHandler(string username, string password)
        {
            _username = username;
            _password = password;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            return await request.AuthoriseAndResend(base.SendAsync, _username, _password, cancellationToken);
        }
    }
}