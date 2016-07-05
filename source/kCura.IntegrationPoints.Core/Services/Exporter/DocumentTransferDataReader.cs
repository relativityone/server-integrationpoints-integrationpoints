using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		public const int FETCH_ARTIFACTDTOS_BATCH_SIZE = 200;

		private static readonly string _nativeDocumentArtifactIdColumn = "DocumentArtifactID";
		private static readonly string _nativeFileNameColumn = "Filename";
		private static readonly string _nativeLocationColumn = "Location";
		private static readonly string _separator = ",";

		private readonly IExporterService _relativityExporterService;
		private readonly Dictionary<int, string> _nativeFileLocations;
		private readonly Dictionary<int, string> _nativeFileNames;
		private readonly ICoreContext _context;
		private readonly IScratchTableRepository[] _scratchTableRepositories;
		private readonly int _folderPathFieldSourceArtifactId;

		/// used as a flag to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;

		public DocumentTransferDataReader(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			ICoreContext context,
			IScratchTableRepository[] scratchTableRepositories) :
			base(GenerateDataColumnsFromFieldEntries(fieldMappings))
		{
			_context = context;
			_scratchTableRepositories = scratchTableRepositories;
			_relativityExporterService = relativityExportService;
			_nativeFileLocations = new Dictionary<int, string>();
			_nativeFileNames = new Dictionary<int, string>();

			FieldMap folderPathInformationField = fieldMappings.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
				_folderPathFieldSourceArtifactId = Int32.Parse(folderPathInformationField.SourceField.FieldIdentifier);
			}
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			ArtifactDTO[] artifacts = _relativityExporterService.RetrieveData(FETCH_ARTIFACTDTOS_BATCH_SIZE);
			List<int> artifactIds = artifacts.Select(x => x.ArtifactId).ToList();

			_scratchTableRepositories.ForEach(repo => repo.AddArtifactIdsIntoTempTable(artifactIds));

			return artifacts;
		}

		protected override bool AllArtifactsFetched()
		{
			return _relativityExporterService.HasDataToRetrieve == false;
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(FieldMap[] mappingFields)
		{
			List<FieldEntry> fields = mappingFields.Select(field => field.SourceField).ToList();

			// we will always import this native file location
			fields.Add(new FieldEntry()
			{
				DisplayName = IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
				FieldType = FieldType.String
			});
			fields.Add(new FieldEntry
			{
				DisplayName = IntegrationPoints.Contracts.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				FieldIdentifier = IntegrationPoints.Contracts.Constants.SPECIAL_FILE_NAME_FIELD,
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
					DisplayName = IntegrationPoints.Contracts.Constants.SPECIAL_FOLDERPATH_FIELD_NAME,
					FieldIdentifier = IntegrationPoints.Contracts.Constants.SPECIAL_FOLDERPATH_FIELD,
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

		public override object GetValue(int i)
		{
			Object result = null;
			string fieldIdentifier = GetName(i);
			int fieldArtifactId = -1;
			bool success = Int32.TryParse(fieldIdentifier, out fieldArtifactId);

			if (success)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
				return retrievedField.Value;
			}
			else if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(_folderPathFieldSourceArtifactId);
				return retrievedField.Value;
			}
			else
			{
				// we will have to go and get native file locations when the reader fetch a new collection of documents.
				if (_readingArtifactIdsReference != ReadingArtifactIDs)
				{
					_readingArtifactIdsReference = ReadingArtifactIDs;
					string documentArtifactIds = String.Join(_separator, ReadingArtifactIDs);
					kCura.Data.DataView dataView = FileQuery.RetrieveNativesForDocuments(_context, documentArtifactIds);

					for (int index = 0; index < dataView.Table.Rows.Count; index++)
					{
						DataRow row = dataView.Table.Rows[index];
						int nativeDocumentArtifactID = (int)row[_nativeDocumentArtifactIdColumn];
						string nativeFileLocation = (string)row[_nativeLocationColumn];
						string nativeFileName = (string)row[_nativeFileNameColumn];
						_nativeFileLocations.Add(nativeDocumentArtifactID, nativeFileLocation);
						_nativeFileNames.Add(nativeDocumentArtifactID, nativeFileName);
					}
				}

				switch (fieldIdentifier)
				{
					case IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD:
						if (_nativeFileLocations.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileLocations[CurrentArtifact.ArtifactId];
							_nativeFileLocations.Remove(CurrentArtifact.ArtifactId);
						}
						break;

					case IntegrationPoints.Contracts.Constants.SPECIAL_FILE_NAME_FIELD:
						if (_nativeFileNames.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileNames[CurrentArtifact.ArtifactId];
							_nativeFileNames.Remove(CurrentArtifact.ArtifactId);
						}
						break;
				}
			}
			return result;
		}
	}
}