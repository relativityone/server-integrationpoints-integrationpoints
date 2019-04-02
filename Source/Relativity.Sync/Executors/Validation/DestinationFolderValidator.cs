using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Folder;
using Relativity.Services.Folder;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DestinationFolderValidator : IValidator
	{
		private const string _ARTIFACT_NOT_EXIST_MESSAGE = "Destination folder does not exist. Folder artifact ID: {0}";
		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;

		public DestinationFolderValidator(IDestinationServiceFactoryForUser destinationServiceFactoryForUser)
		{
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult validationResult = new ValidationResult();

			using (IFolderManager folderManager = await _destinationServiceFactoryForUser.CreateProxyAsync<IFolderManager>().ConfigureAwait(false))
			{
				FolderStatus folderStatus = await folderManager.GetAccessStatusAsync(configuration.DestinationWorkspaceArtifactId, configuration.DestinationFolderArtifactId).ConfigureAwait(false);
				if (!folderStatus.Exists)
				{
					string message = string.Format(CultureInfo.InvariantCulture, _ARTIFACT_NOT_EXIST_MESSAGE, configuration.DestinationFolderArtifactId);
					validationResult.Add(message);
				}
			}

			return validationResult;
		}
	}
}