using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class PermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		public PermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer) : base(serializer)
		{
			_repositoryFactory = repositoryFactoryFactory;
		}

		public override string Key => IntegrationPointPermissionValidator.GetProviderValidatorKey(
			IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
			IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID
		);

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			var exportSettings = _serializer.Deserialize<ExportUsingSavedSearchSettings>(model.SourceConfiguration);

			int workspaceId = exportSettings.SourceWorkspaceArtifactId;

			IntegrationPoints.Core.Models.ExportSettings.ExportType exportType;
			if (!Enum.TryParse(exportSettings.ExportType, out exportType))
			{
				throw new ArgumentException("Failed to retrieve ExportType from export settings.");
			}

			var permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceId);

			if ((exportType == IntegrationPoints.Core.Models.ExportSettings.ExportType.Folder)
				|| (exportType == IntegrationPoints.Core.Models.ExportSettings.ExportType.FolderAndSubfolders))
			{
				var hasFolderPermission = permissionRepository.UserHasArtifactInstancePermission(
					(int)ArtifactType.Folder, exportSettings.FolderArtifactId, ArtifactPermission.View);

				if (!hasFolderPermission)
				{
					result.Add(FileDestinationProviderValidationMessages.EXPORT_FOLDER_NO_VIEW);
				}
			}
			else if (exportType == IntegrationPoints.Core.Models.ExportSettings.ExportType.ProductionSet)
			{
				var hasProductionPermission = permissionRepository.UserHasArtifactInstancePermission(
					(int)ArtifactType.Production, exportSettings.ProductionId, ArtifactPermission.View);
				if (!hasProductionPermission)
				{
					result.Add(FileDestinationProviderValidationMessages.EXPORT_PRODUCTION_NO_VIEW);
				}
			}

			return result;
		}
	}
}
