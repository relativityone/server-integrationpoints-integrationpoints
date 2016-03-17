using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Readers;
using kCura.IntegrationPoints.Data.Queries;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : RelativityReaderBase
	{
		private readonly IExporterService _relativityExporterService;
		private Dictionary<int, string> _nativeFileLocation;
		private readonly DirectSqlCallHelper _sqlHelper;

		public DocumentTransferDataReader(
			IExporterService relativityExportService,
			IEnumerable<FieldEntry> fieldEntries,
			DirectSqlCallHelper helper) :
			base(GenerateDataColumnsFromFieldEntries(fieldEntries))
		{
			_relativityExporterService = relativityExportService;
			_nativeFileLocation = _nativeFileLocation ?? new Dictionary<int, string>();
			_sqlHelper = helper;
		}

		protected override ArtifactDTO[] FetchArtifactDTOs()
		{
			return _relativityExporterService.RetrieveData(50);
		}

		protected override bool AllArtifactsFetched()
		{
			return _relativityExporterService.HasDataToRetrieve == false;
		}

		private static DataColumn[] GenerateDataColumnsFromFieldEntries(IEnumerable<FieldEntry> fieldEntries)
		{
			// we will always import this native file location
			List<FieldEntry> fields = fieldEntries.ToList();
			fields.Add(new FieldEntry()
			{
				DisplayName = IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				FieldIdentifier = kCura.IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD,
				FieldType = FieldType.String
			});

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
			else if (fieldIdentifier == IntegrationPoints.Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
			{
				if (_nativeFileLocation.ContainsKey(CurrentArtifact.ArtifactId) == false)
				{
					_nativeFileLocation = _sqlHelper.GetFileLocation(ReadingArtifactIDs);
				}
				result = _nativeFileLocation[CurrentArtifact.ArtifactId];
			}
			return result;
		}
	}
}