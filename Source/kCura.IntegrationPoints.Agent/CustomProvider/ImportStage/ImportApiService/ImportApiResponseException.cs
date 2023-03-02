using System;
using Relativity.Import.V1;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportApiResponseException : Exception
    {
        /// <summary>
        /// Parameterless constructor for tests purposes only.
        /// </summary>
        public ImportApiResponseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportApiResponseException"/> class.
        /// </summary>
        public ImportApiResponseException(Response response) : base($"ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}")
        {
            Response = response;
        }

        public Response Response { get; }
    }
}
