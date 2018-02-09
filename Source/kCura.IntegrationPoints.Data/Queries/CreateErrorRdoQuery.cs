using System;
using kCura.IntegrationPoints.Data.Logging;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateErrorRdoQuery
	{
		private const string _TRUNCATED_TEMPLATE = "(Truncated) {0}";
		public const int MAX_ERROR_LEN = 2000;
		public const int MAX_SURCE_LEN = 255;

		private readonly IAPILog _logger;
		private readonly IRsapiClientWithWorkspaceFactory _rsapiClientFactory;
		private readonly ISystemEventLoggingService _systemEventLoggingService;
		
		public CreateErrorRdoQuery(IRsapiClientWithWorkspaceFactory rsapiClientFactory, IAPILog logger, ISystemEventLoggingService systemEventLoggingService)
		{
			_rsapiClientFactory = rsapiClientFactory;
			_logger = logger.ForContext<CreateErrorRdoQuery>();
			_systemEventLoggingService = systemEventLoggingService;
		}

		public void Execute(Error errDto)
		{
			if (errDto.Message?.Length > MAX_ERROR_LEN)
			{
				errDto.Message = TruncateMessage(errDto.Message);
			}
			if (errDto.Source?.Length > MAX_SURCE_LEN)
			{
				errDto.Source = errDto.Source.Substring(0, MAX_SURCE_LEN);
			}
			using (IRSAPIClient client = _rsapiClientFactory.CreateAdminClient())
			{
				try
				{
					client.Repositories.Error.Create(errDto);
				}
				catch (Exception ex)
				{
					_systemEventLoggingService.WriteErrorEvent(errDto.Source, "Application", ex);
					LogErrorMessage(errDto, ex);
				}
			}
		}

		private string TruncateMessage(string message)
		{
			int truncatedLength = MAX_ERROR_LEN - _TRUNCATED_TEMPLATE.Length;
			return string.Format(_TRUNCATED_TEMPLATE, message.Substring(0, truncatedLength));
		}

		private void LogErrorMessage(Error errDto, Exception ex)
		{
			string message = $"An Error occured during creation of ErrorRDO for workspace: {errDto.Workspace?.ArtifactID}.{Environment.NewLine}"
			+ $"Source: {errDto.Source}. {Environment.NewLine}"
			+ $"Error message: {errDto.Message}. {Environment.NewLine}"
			+ $"Stacktrace: {ex.StackTrace}";

			_logger.LogError(ex, message);
		}
	}
}