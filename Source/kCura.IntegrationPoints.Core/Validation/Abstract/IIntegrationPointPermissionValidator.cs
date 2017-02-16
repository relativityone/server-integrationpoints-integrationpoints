using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
	public interface IIntegrationPointPermissionValidator : IIntegrationPointValidator
	{
		ValidationResult ValidateSave(IntegrationPointModelBase model, SourceProvider sourceProvider, 
			DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid);

		ValidationResult ValidateViewErrors(int workspaceArtifactId);

		ValidationResult ValidateStop(IntegrationPointModelBase model, SourceProvider sourceProvider,
			DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid);
	}
}
