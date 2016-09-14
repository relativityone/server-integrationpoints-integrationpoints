using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters.Validator;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class PaddingValidator : IPaddingValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public PaddingValidator(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public ExportSettingsValidationResult Validate(int workspaceId, ExportFile exportFile)
		{
			//Logic extracted from SharedLibrary
			var totalFiles = GetTotalExportItemsCount(workspaceId, exportFile.ArtifactID, exportFile.StartAtDocumentNumber);

			var currentVolumeNumber = exportFile.VolumeInfo.VolumeStartNumber;
			var currentSubdirectoryNumber = exportFile.VolumeInfo.SubdirectoryStartNumber;

			var subdirectoryNumberPaddingWidth = (int) Math.Floor(Math.Log10(currentSubdirectoryNumber + 1) + 1);
			var volumeNumberPaddingWidth = (int) Math.Floor(Math.Log10(currentVolumeNumber + 1) + 1);
			var totalFilesNumberPaddingWidth = (int) Math.Floor(Math.Log10(totalFiles + currentVolumeNumber + 1) + 1);
			var volumeLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, volumeNumberPaddingWidth);
			var subdirectoryLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, subdirectoryNumberPaddingWidth);

			var warningValidator = new PaddingWarningValidator();
			var isValid = warningValidator.IsValid(exportFile, volumeLabelPaddingWidth, subdirectoryLabelPaddingWidth);

			return new ExportSettingsValidationResult
			{
				IsValid = isValid,
				Message = warningValidator.ErrorMessages
			};
		}

		private int GetTotalExportItemsCount(int workspaceId, int savedSearchArtifactId, int startExportAtRecord)
		{
			var savedSearchRepo = _repositoryFactory.GetSavedSearchRepository(workspaceId, savedSearchArtifactId);
			var totalDocsCount = savedSearchRepo.GetTotalDocsCount();
			return Math.Max(totalDocsCount - (startExportAtRecord - 1), 0);
		}
	}
}