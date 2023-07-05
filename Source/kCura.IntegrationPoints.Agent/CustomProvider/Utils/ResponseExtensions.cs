using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using Relativity.Import.V1;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Utils
{
    internal static class ResponseExtensions
    {
        public static void Validate(this Response response, string customMessage = "")
        {
            if (!response.IsSuccess)
            {
                throw new ImportApiResponseException(response);
            }
        }

        public static T UnwrapOrThrow<T>(this ValueResponse<T> response, string customMessage = "")
        {
            response.Validate(customMessage);

            return response.Value;
        }
    }
}