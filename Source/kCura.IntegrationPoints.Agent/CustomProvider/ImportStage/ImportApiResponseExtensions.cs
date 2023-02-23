using Relativity.Import.V1;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal static class ImportApiResponseExtensions
    {
        public static void ValidateOrThrow(this Response response)
        {
            if (!response.IsSuccess)
            {
                throw new ImportApiResponseException(response);
            }
        }
    }
}
