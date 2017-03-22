using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : ExportTransferDataReaderBase
	{
		private static readonly string _nativeDocumentArtifactIdColumn = "DocumentArtifactID";
		private static readonly string _nativeFileNameColumn = "Filename";
		private static readonly string _nativeLocationColumn = "Location";
		private static readonly string _separator = ",";

		private readonly Dictionary<int, string> _nativeFileLocations;
		private readonly Dictionary<int, string> _nativeFileNames;

		/// used as a flag to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;

		public DocumentTransferDataReader(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			ICoreContext context,
			IScratchTableRepository[] scratchTableRepositories,
			bool folderPathDynamic) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories, folderPathDynamic)
		{
			_nativeFileLocations = new Dictionary<int, string>();
			_nativeFileNames = new Dictionary<int, string>();
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
			else if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(_folderPathFieldSourceArtifactId);
				return retrievedField.Value;
			}
			else if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD)
			{
				return CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME).Value;
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
					case IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD:
						if (_nativeFileLocations.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileLocations[CurrentArtifact.ArtifactId];
							_nativeFileLocations.Remove(CurrentArtifact.ArtifactId);
						}
						break;

					case IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD:
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