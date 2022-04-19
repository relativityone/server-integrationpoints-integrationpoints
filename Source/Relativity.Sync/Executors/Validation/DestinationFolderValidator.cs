using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Folder;
using Relativity.Services.Folder;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DestinationFolderValidator : IValidator
	{
		private const string _ARTIFACT_NOT_EXIST_MESSAGE = "Destination folder does not exist. Folder artifact ID: {0}";
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		private readonly IAPILog _logger;

		public DestinationFolderValidator(IDestinationServiceFactoryForUser destinationServiceFactoryForUser, IAPILog logger)
		{
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating destination folder. Workspace Artifact ID: {destinationWorkspaceArtifactId} Folder Artifact ID: {destinationFolderArtifactId}",
				configuration.DestinationWorkspaceArtifactId, configuration.DestinationFolderArtifactId);
			ValidationResult validationResult = new ValidationResult();

			try
			{
				using (IFolderManager folderManager = await _destinationServiceFactoryForUser.CreateProxyAsync<IFolderManager>().ConfigureAwait(false))
				{
					FolderStatus folderStatus = await folderManager.GetAccessStatusAsync(configuration.DestinationWorkspaceArtifactId, configuration.DestinationFolderArtifactId).ConfigureAwait(false);
					if (!folderStatus.Exists)
					{
						_logger.LogError("Folder with id {destinationFolderArtifactId} does not exist in destination workspace {destinationWorkspaceArtifactId}", configuration.DestinationFolderArtifactId,
							configuration.DestinationWorkspaceArtifactId);
						string message = string.Format(CultureInfo.InvariantCulture, _ARTIFACT_NOT_EXIST_MESSAGE, configuration.DestinationFolderArtifactId);
						validationResult.Add(message);
					}
				}
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred when validating destination folder";
				_logger.LogError(ex, message);
				throw;
			}

			return validationResult;
		}

		public bool ShouldValidate(ISyncPipeline pipeline) => !pipeline.IsNonDocumentPipeline();
	}
}
