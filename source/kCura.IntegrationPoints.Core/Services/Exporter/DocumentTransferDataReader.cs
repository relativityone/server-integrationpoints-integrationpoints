using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using Relativity.Core;
using Relativity.Core.Service;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private static string DocumentArtifactId = "DocumentArtifactId";
		private static string FileLocation = "Location";
		private static string Separator = ",";

		private readonly IExporterService _relativityExporterService;
		private readonly Dictionary<int, string> _nativeFileLocation;
		private readonly ICoreContext _context;
		private readonly int _folderPathFieldSourceArtifactId;

		/// used to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;

		public DocumentTransferDataReader(
			IExporterService relativityExportService,
			FieldMap[] mappingFields,
			ICoreContext context) :
			base(GenerateDataColumnsFromFieldEntries(mappingFields))
		{
			_context = context;
			_relativityExporterService = relativityExportService;
			_nativeFileLocation = new Dictionary<int, string>();

			FieldMap folderPathInformationField = mappingFields.FirstOrDefault(mappedField => mappedField.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (folderPathInformationField != null)
			{
				_folderPathFieldSourceArtifactId = Int32.Parse(folderPathInformationField.SourceField.FieldIdentifier);
			}
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			return _relativityExporterService.RetrieveData(50);
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
			else if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
			{
				if (_readingArtifactIdsReference != ReadingArtifactIDs)
				{
					_readingArtifactIdsReference = ReadingArtifactIDs;
					string documentArtifactIds = String.Join(Separator, ReadingArtifactIDs);
					kCura.Data.DataView dataView = FileQuery.RetrieveNativesForDocuments(_context, documentArtifactIds);

					for (int index = 0; index < dataView.Table.Rows.Count; index++)
					{
						DataRow row = dataView.Table.Rows[index];
						int id = (int)row[DocumentArtifactId];
						string location = (string)row[FileLocation];
						_nativeFileLocation.Add(id, location);
					}
				}

				if (_nativeFileLocation.ContainsKey(CurrentArtifact.ArtifactId))
				{
					result = _nativeFileLocation[CurrentArtifact.ArtifactId];
					_nativeFileLocation.Remove(CurrentArtifact.ArtifactId);
				}
			}
			return result;
		}
	}
}