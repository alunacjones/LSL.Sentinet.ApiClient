namespace LSL.Sentinet.ApiClient.DependencyInjection
{
    /// <summary>
    /// SentinetApiClient configurable settings
    /// </summary>
    public class SentinetApiOptions
    {
        /// <summary>
        /// The base url of the API
        /// </summary>
        /// <value></value>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The username to login as
        /// </summary>
        /// <remarks>
        /// NEVER store this in a configuration file
        /// </remarks>
        /// <value></value>
        public string Username { get; set; }

        /// <summary>
        /// The password of the username to login as
        /// </summary>
        /// <remarks>
        /// NEVER store this in a configuration file
        /// </remarks>
        /// <value></value>
        public string Password { get; set; }
    }
}