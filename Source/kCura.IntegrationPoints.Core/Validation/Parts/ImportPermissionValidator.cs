using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class ImportPermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		public ImportPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer) : base(serializer)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}
		public override string Key => Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString();

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			DestinationConfiguration destinationConfiguration = _serializer.Deserialize<DestinationConfiguration>(model.DestinationConfiguration);

			var permissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.SourceWorkspaceArtifactId);

			if (!permissionRepository.UserCanImport())
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);
			}

			if (!permissionRepository.UserHasArtifactTypePermissions(destinationConfiguration.ArtifactTypeId, 
				new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create }))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			return result;
		}
	}
}
