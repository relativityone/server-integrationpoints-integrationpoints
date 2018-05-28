﻿using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces
{
	public interface IRelativityProviderDestinationWorkspaceExistenceValidator
	{
		ValidationResult Validate(int workspaceId);
	}
}
