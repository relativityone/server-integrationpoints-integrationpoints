using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Images
{
	public class ImageTransferDataReader : ExportTransferDataReaderBase
	{
		public ImageTransferDataReader(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories, false)
		{ }

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] artifacts = RelativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);

			List<int> artifactIds = artifacts.Select(x => x.ArtifactId).Distinct().ToList();
			foreach (IScratchTableRepository repository in ScratchTableRepositories)
			{
				repository.AddArtifactIdsIntoTempTable(artifactIds);
			}

			return artifacts;
		}

		public override object GetValue(int i)
		{
			string fieldIdentifier = GetName(i);

			int fieldArtifactId;
			bool isFieldIdentifierNumericValue = int.TryParse(fieldIdentifier, out fieldArtifactId);
			if (isFieldIdentifierNumericValue)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
				return retrievedField.Value;
			}

			switch (fieldIdentifier)
			{
				case IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD:
					ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(FolderPathFieldSourceArtifactId);
					return retrievedField.Value;
				case IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD:
					ArtifactFieldDTO fileLocationField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME);
					return fileLocationField.Value;
				case IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD:
					ArtifactFieldDTO nameField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME);
					return nameField.Value;
				default:
					return null;
			}
		}
	}
}