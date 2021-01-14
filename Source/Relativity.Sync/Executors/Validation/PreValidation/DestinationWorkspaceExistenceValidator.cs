﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation.PreValidation
{
	internal sealed class DestinationWorkspaceExistenceValidator : IPreValidator
	{
		private const string _DESTINATION_WORKSPACE_DOES_NOT_EXIST_MESSAGE = "Destination Workspace does not exist";

		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceExistenceValidator(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IPreValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validation if Destination Workspace {workspaceId} exists", configuration.DestinationWorkspaceArtifactId);

			ValidationResult result = new ValidationResult();
			try
			{
				using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
				{
					WorkspaceRef destinationWorkspace = new WorkspaceRef(configuration.DestinationWorkspaceArtifactId);

					bool exists = await workspaceManager.WorkspaceExists(destinationWorkspace).ConfigureAwait(false);
					if (!exists)
					{
						result.Add(_DESTINATION_WORKSPACE_DOES_NOT_EXIST_MESSAGE);
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
