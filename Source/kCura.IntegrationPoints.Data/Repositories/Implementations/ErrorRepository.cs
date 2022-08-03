using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Error;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ErrorRepository : IErrorRepository
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public ErrorRepository(IHelper helper)
        {
            _helper = helper;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ErrorRepository>();
        }

        public void Create(IEnumerable<ErrorDTO> errors)
        {
            Error[] errorArtifacts = errors.Select(ConvertErrorDto).ToArray();

            using (IErrorManager errorManager = _helper.GetServicesManager().CreateProxy<IErrorManager>(ExecutionIdentity.CurrentUser))
            {
                foreach (Error error in errorArtifacts)
                {
                    try
                    {
                        errorManager.CreateSingleAsync(error).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected exception occurred when creating error entry. Error details: {@error}", error);
                    }
                }
            }
        }

        private Error ConvertErrorDto(ErrorDTO errorToConvert)
        {
            var convertedError = new Error()
            {
                Message = errorToConvert.Message,
                FullError = errorToConvert.FullText,
                Source = errorToConvert.Source,
                Workspace = new WorkspaceRef(errorToConvert.WorkspaceId),
                Server = Environment.MachineName
            };

            return convertedError;
        }
    }
}
