using System;
using Relativity.Import.V1;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportApiResponseException : Exception
    {
        public ImportApiResponseException(Response response) : base($"ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}")
        {
            Response = response;
        }

        public Response Response { get; }
    }
}
