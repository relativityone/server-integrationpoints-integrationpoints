using System;
using Relativity.API;
using Relativity.Services.Error;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class CreateErrorRdoQuery
    {
        private const int _MAX_ERROR_LENGTH = 2000;
        private const int _MAX_SOURCE_LENGTH = 255;
        private const string _TRUNCATED_TEMPLATE = "(Truncated) {0}";
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public CreateErrorRdoQuery(IServicesMgr servicesMgr, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _logger = logger.ForContext<CreateErrorRdoQuery>();
        }

        public void LogError(Error errorDto)
        {
            if (errorDto.Message?.Length > _MAX_ERROR_LENGTH)
            {
                errorDto.Message = TruncateMessage(errorDto.Message);
            }

            if (errorDto.Source?.Length > _MAX_SOURCE_LENGTH)
            {
                errorDto.Source = errorDto.Source.Substring(0, _MAX_SOURCE_LENGTH);
            }

            try
            {
                using (IErrorManager errorManager = _servicesMgr.CreateProxy<IErrorManager>(ExecutionIdentity.System))
                {
                    errorManager.CreateSingleAsync(errorDto).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                LogErrorMessage(errorDto, ex);
            }
        }

        private string TruncateMessage(string message)
        {
            int truncatedLength = _MAX_ERROR_LENGTH - _TRUNCATED_TEMPLATE.Length;
            return string.Format(_TRUNCATED_TEMPLATE, message.Substring(0, truncatedLength));
        }

        private void LogErrorMessage(Error errorDto, Exception ex)
        {
            string message = $"An Error occured during creation of ErrorRDO for workspace: {errorDto.Workspace?.ArtifactID}.{Environment.NewLine}"
            + $"Source: {errorDto.Source}. {Environment.NewLine}"
            + $"Error message: {errorDto.Message}. {Environment.NewLine}"
            + $"Stacktrace: {ex.StackTrace}";

            _logger.LogError(ex, message);
        }
    }
}
