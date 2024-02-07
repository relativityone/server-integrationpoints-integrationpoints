namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the response for the uninstallation of a source provider.
    /// </summary>
    public class UninstallProviderResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the uninstallation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message in case of failure.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates response which indicates success
        /// </summary>
        public UninstallProviderResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Creates response which indicates failure
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public UninstallProviderResponse(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
