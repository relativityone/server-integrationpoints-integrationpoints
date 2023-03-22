using Relativity.Import.V1;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Extensions
{
    public static class ResponseExtensions
    {
        public static void Validate(this Response response)
        {
            if (response?.IsSuccess != true)
            {
                throw new ImportApiResponseException(response);
            }
        }
    }
}