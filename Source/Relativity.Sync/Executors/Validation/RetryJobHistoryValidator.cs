using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class RetryJobHistoryValidator : IValidator
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly Guid _jobHistoryGuid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");

		public RetryJobHistoryValidator(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult validationResult = new ValidationResult();

			if (configuration.JobHistoryToRetryId != null)
			{
				_logger.LogVerbose("Validating JobHistoryToRetry {artifactId} exists", configuration.JobHistoryToRetryId.Value);

				try
				{
					using (IObjectManager objectManager =
						await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
					{
						QueryRequest request = new QueryRequest
						{
							ObjectType = new ObjectTypeRef
							{
								Guid = _jobHistoryGuid
							},
							Condition = $"'ArtifactId' == {configuration.JobHistoryToRetryId.Value}"
						};

						QueryResult result = await objectManager.QueryAsync(configuration.SourceWorkspaceArtifactId, request, 1, 1, token, new EmptyProgress<ProgressReport>())
							.ConfigureAwait(false);

						if (result.ResultCount == 0)
						{
							string messageTemplate =
								"JobHistory with ArtifactId = {jobHistoryToRetryId} does not exist";
							_logger.LogError(messageTemplate, configuration.JobHistoryToRetryId.Value);
							validationResult.Add($"JobHistory with ArtifactId = {configuration.JobHistoryToRetryId.Value} does not exist");
						}
					}
				}
				catch (Exception e)
				{
					string message = "Failed to validate JobHistoryToRetry (ArtifactId = {artifactID})";
					_logger.LogError(e, message, configuration.JobHistoryToRetryId.Value);
					validationResult.Add(string.Format($"Failed to validate JobHistoryToRetry (ArtifactId = {configuration.JobHistoryToRetryId.Value}"));
				}
			}
			else
			{
				_logger.LogInformation("Skipping JobHistoryToRetry validation because it's not set in configuration");
			}

			return validationResult;
		}
	}
}
