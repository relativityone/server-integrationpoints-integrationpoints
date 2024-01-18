namespace Relativity.IntegrationPoints.Services
{
    public class UninstallProviderResponse
    {
        public bool Success { get; set; }

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
        public UninstallProviderResponse(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
