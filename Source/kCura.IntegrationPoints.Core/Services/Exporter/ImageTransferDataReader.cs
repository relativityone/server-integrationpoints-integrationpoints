using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ImageTransferDataReader : ExportTransferDataReaderBase
	{
		public ImageTransferDataReader(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			ICoreContext context,
			IScratchTableRepository[] scratchTableRepositories) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories)
		{
			
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] artifacts = _relativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);
			//TODO: Image identify if is needed for image transfer
			//List<int> artifactIds = artifacts.Select(x => x.ArtifactId).ToList();
			//_scratchTableRepositories.ForEach(repo => repo.AddArtifactIdsIntoTempTable(artifactIds));



			return artifacts;
		}

		public override object GetValue(int i)
		{
			string fieldIdentifier = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);

			if (success)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
				return retrievedField.Value;
			}
			else if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(_folderPathFieldSourceArtifactId);
				return retrievedField.Value;
			}
			else
			{
				switch (fieldIdentifier)
				{
					case IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD:
						ArtifactFieldDTO fileLocationField = CurrentArtifact.GetFieldForName(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME);
						return fileLocationField.Value;

					case IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD:
						ArtifactFieldDTO nameField = CurrentArtifact.GetFieldForName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME);
						return nameField.Value;
				}
			}
			return null;
		}
	}
}