using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LSL.Sentinet.ApiClient
{
    internal static class SentinetAuthServceExtensions
    {        
        private static readonly Regex _baseUrlMatcher = new Regex(@".*/RepositoryService.svc/", RegexOptions.Compiled);
        
        internal static async Task<HttpResponseMessage> AuthoriseAndResend(
            this HttpRequestMessage source,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncDelegate,
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            var baseUrl = _baseUrlMatcher.Match(source.RequestUri.ToString()).Value;
            
            var authRequest = new HttpRequestMessage(
                HttpMethod.Get, 
                $"{baseUrl}LogOn?userName={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}");

            var authResponse = await sendAsyncDelegate(authRequest, cancellationToken).ConfigureAwait(false);
            authResponse.EnsureSuccessStatusCode();
            var content = await authResponse.Content.ReadAsStringAsync();

            if (!content.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new HttpRequestException("Unable to log into the Sentinet API with the provided credentials");
            }

            return await sendAsyncDelegate(source, cancellationToken).ConfigureAwait(false);
        }
    }
}