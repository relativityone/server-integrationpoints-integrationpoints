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

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public abstract class ExportTransferDataReaderBase : RelativityReaderBase
	{
		protected readonly BaseServiceContext Context;

		protected readonly IExporterService RelativityExporterService;
		protected readonly int FolderPathFieldSourceArtifactId;
		protected readonly IScratchTableRepository[] ScratchTableRepositories;
		public const int FETCH_ARTIFACTDTOS_BATCH_SIZE = 200;

		protected ExportTransferDataReaderBase(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories,
			bool useDynamicFolderPath) :
				base(GenerateDataColumnsFromFieldEntries(fieldMappings, useDynamicFolderPath))
		{
			Context = context;
			ScratchTableRepositories = scratchTableRepositories;
			RelativityExporterService = relativityExportService;

			FieldMap folderPathInformationField = fieldMappings.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
				FolderPathFieldSourceArtifactId = Int32.Parse(folderPathInformationField.SourceField.FieldIdentifier);
			}
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{

			ArtifactDTO[] artifacts = RelativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);
			List<int> artifactIds = artifacts.Select(x => x.ArtifactId).ToList();

			ScratchTableRepositories.ForEach(repo => repo.AddArtifactIdsIntoTempTable(artifactIds));

			return artifacts;
		}

		protected override bool AllArtifactsFetched()
		{
			return RelativityExporterService.HasDataToRetrieve == false;
		}

		protected static DataColumn[] GenerateDataColumnsFromFieldEntries(FieldMap[] mappingFields, bool useDynamicFolderPath)
		{
			List<FieldEntry> fields = mappingFields.Select(field => field.SourceField).ToList();

			// we will always import this native file location
			fields.Add(new FieldEntry()
			{
				DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
				FieldType = FieldType.String
			});
			fields.Add(new FieldEntry
			{
				DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD,
				FieldType = FieldType.String
			});

			// in case we found folder path info
			FieldMap folderPathInformationField = mappingFields.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
				if (folderPathInformationField.DestinationField.FieldIdentifier == null)
				{
					fields.Remove(folderPathInformationField.SourceField);
				}

				fields.Add(new FieldEntry()
				{
					DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME,
					FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD,
					FieldType = FieldType.String
				});
			}
			else if (useDynamicFolderPath)
			{
				fields.Add(new FieldEntry()
				{
					DisplayName = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME,
					FieldIdentifier = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD,
					FieldType = FieldType.String
				});
			}

			return fields.Select(x => new DataColumn(x.FieldIdentifier)).ToArray();
		}

		public override string GetDataTypeName(int i)
		{
			return GetFieldType(i).ToString();
		}

		public override Type GetFieldType(int i)
		{
			string fieldIdentifier = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);
			object value = null;
			if (success)
			{
				value = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId).Value;
			}
			return value == null ? typeof(object) : value.GetType();
		}

	}
}