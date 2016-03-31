using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.Core;
using Relativity.Core.DTO;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		public const int FETCH_ARTIFACTDTOS_BATCH_SIZE = 50;
		private static string Separator = ",";

		private readonly IExporterService _relativityExporterService;
		private readonly Dictionary<int, File> _files;
		private readonly ICoreContext _context;
		private readonly ITempDocumentFactory _tempDocumentFactory;
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly int _folderPathFieldSourceArtifactId;

		/// used as a flag to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;

		public DocumentTransferDataReader(IExporterService relativityExportService,FieldMap[] fieldMappings,
			ICoreContext context, string jobDetails) : base(GenerateDataColumnsFromFieldEntries(fieldMappings))
		{
			_context = context;
			_relativityExporterService = relativityExportService;
			_files = new Dictionary<int, File>();
			//todo: resolve TempDocumentFactory to make it unit testable 
			_tempDocumentFactory = new TempDocumentFactory();
			_tempDocHelper = _tempDocumentFactory.GetTableCreationHelper(context, Constants.IntegrationPoints.Temporary_Document_Table_Name, jobDetails);

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
	
			_tempDocHelper.CreateTemporaryDocTable(artifactIds);
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
				result = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId).Value;
			}
			else if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
				result = CurrentArtifact.GetFieldForIdentifier(_folderPathFieldSourceArtifactId).Value;
			}
			else if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD || fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_FILE_NAME_FIELD)
			{
				// we will have to go and get native file locations when the reader fetch a new collection of documents.
				if (_readingArtifactIdsReference != ReadingArtifactIDs)
				{
					_readingArtifactIdsReference = ReadingArtifactIDs;
					string documentArtifactIds = String.Join(Separator, ReadingArtifactIDs);
					kCura.Data.DataView dataView = FileQuery.RetrieveNativesForDocuments(_context, documentArtifactIds);

					for (int index = 0; index < dataView.Table.Rows.Count; index++)
					{
						DataRow row = dataView.Table.Rows[index];
						File file = new File(row);
						_files.Add(file.DocumentArtifactID, file);
					}
				}

				if (_files.ContainsKey(CurrentArtifact.ArtifactId))
				{
					File file = _files[CurrentArtifact.ArtifactId];

					if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
					{
						result = file.Location;
					}
					else
					{
						result = file.Filename;
					}
				}
			}
			return result;
		}
	}
}