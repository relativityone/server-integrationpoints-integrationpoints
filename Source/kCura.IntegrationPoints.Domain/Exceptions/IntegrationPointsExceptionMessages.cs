namespace kCura.IntegrationPoints.Domain.Exceptions
{
    public static class IntegrationPointsExceptionMessages
    {
        public const string ERROR_OCCURED_CONTACT_ADMINISTRATOR = "An error occurred. Please contact administrator.";

        public static string CreateErrorMessageRetryOrContactAdministrator(string operationName)
        {
            return $"An error occurred {operationName}. Please retry or contact administrator";
        }
    }
}
