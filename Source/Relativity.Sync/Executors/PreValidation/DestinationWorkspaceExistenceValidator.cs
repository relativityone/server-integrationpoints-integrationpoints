using Relativity.API;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.PreValidation
{
	internal sealed class DestinationWorkspaceExistenceValidator : IPreValidator
	{
		private const string _DESTINATION_WORKSPACE_DOES_NOT_EXIST_MESSAGE = "Destination Workspace {0} does not exist";

		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IAPILog _logger;

		public DestinationWorkspaceExistenceValidator(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IPreValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validation if Destination Workspace {workspaceId} exists", configuration.DestinationWorkspaceArtifactId);

			ValidationResult result = new ValidationResult();
			try
			{
				using (IWorkspaceManager workspaceManager = await _serviceFactoryForAdmin.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
				{
					WorkspaceRef destinationWorkspace = new WorkspaceRef(configuration.DestinationWorkspaceArtifactId);

					bool exists = await workspaceManager.WorkspaceExists(destinationWorkspace).ConfigureAwait(false);
					if (!exists)
					{
						result.Add(string.Format(CultureInfo.InvariantCulture, _DESTINATION_WORKSPACE_DOES_NOT_EXIST_MESSAGE,
							configuration.DestinationWorkspaceArtifactId));
					}
				}
			}
			catch (Exception ex)
			{
				string message = "Error occurred while checking for workspace existence with artifact ID: {0}";
				_logger.LogError(ex, message, configuration.DestinationWorkspaceArtifactId);
				result.Add(string.Format(CultureInfo.InvariantCulture, message, configuration.DestinationWorkspaceArtifactId));
			}

			return result;
		}
	}
}
