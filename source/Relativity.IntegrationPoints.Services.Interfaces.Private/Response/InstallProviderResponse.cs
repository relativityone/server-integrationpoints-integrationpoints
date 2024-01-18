namespace Relativity.IntegrationPoints.Services
{
    public class InstallProviderResponse
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates response which indicates success
        /// </summary>
        public InstallProviderResponse()
        {
            Success = true;
        }

        /// <summary>
        /// Creates response which indicates failure
        /// </summary>
        public InstallProviderResponse(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
