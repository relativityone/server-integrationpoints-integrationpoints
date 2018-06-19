﻿using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderDestinationWorkspaceExistenceValidator : IRelativityProviderDestinationWorkspaceExistenceValidator
	{
		private readonly IWorkspaceManager _workspaceManager;

		public RelativityProviderDestinationWorkspaceExistenceValidator(IWorkspaceManager workspaceManager)
		{
			_workspaceManager = workspaceManager;
		}
		public ValidationResult Validate(int workspaceId, bool isFederatedInstance)
		{
			var result = new ValidationResult();

			if (!_workspaceManager.WorkspaceExists(workspaceId))
			{
				ValidationMessage message = isFederatedInstance
					? ValidationMessages.FederatedInstanceDestinationWorkspaceNotAvailable
					: ValidationMessages.DestinationWorkspaceNotAvailable;
				result.Add(message);
			}
			return result;
		}
	}
}
