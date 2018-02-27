using System.Collections.Generic;
using System.Data;
using kCura.EDDS.DocumentCompareGateway;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Core;
using Relativity.Toggles;
using FileQuery = Relativity.Core.Service.FileQuery;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : ExportTransferDataReaderBase
	{
		/// used as a flag to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;

		private readonly Dictionary<int, string> _nativeFileLocations;
		private readonly Dictionary<int, string> _nativeFileNames;
		private readonly IILongTextStreamFactory _relativityLongTextStreamFactory;
		private readonly IToggleProvider _toggleProvider;
		private readonly List<ILongTextStream> _openedStreams;


		private static readonly string _nativeDocumentArtifactIdColumn = "DocumentArtifactID";
		private static readonly string _nativeFileNameColumn = "Filename";
		private static readonly string _nativeLocationColumn = "Location";
		private static readonly string _separator = ",";

		public DocumentTransferDataReader(IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories,
			IILongTextStreamFactory longTextStreamFactory,
			IToggleProvider toggleProvider,
			bool useDynamicFolderPath) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories, useDynamicFolderPath)
		{
			_nativeFileLocations = new Dictionary<int, string>();
			_nativeFileNames = new Dictionary<int, string>();
			_relativityLongTextStreamFactory = longTextStreamFactory;
			_toggleProvider = toggleProvider;
			_openedStreams = new List<ILongTextStream>();
		}


		public override object GetValue(int i)
		{
			string fieldIdentifier = GetName(i);

			int fieldArtifactId;
			bool isFieldIdentifierNumeric = int.TryParse(fieldIdentifier, out fieldArtifactId);
			if (isFieldIdentifierNumeric)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
				if (ShouldUseLongTextStream(retrievedField))
				{
					ILongTextStream stream =
						_relativityLongTextStreamFactory.CreateLongTextStream(CurrentArtifact.ArtifactId, fieldArtifactId);
					_openedStreams.Add(stream);
					return stream;
				}

				return retrievedField.Value;
			}

			if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD)
			{
				ArtifactFieldDTO retrievedField = CurrentArtifact.GetFieldForIdentifier(FolderPathFieldSourceArtifactId);
				return retrievedField.Value;
			}
			if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD)
			{
				return CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME).Value;
			}


			// we will have to go and get native file locations when the reader fetch a new collection of documents.
			if (_readingArtifactIdsReference != ReadingArtifactIDs)
			{
				LoadNativeFilesLocationsAndNames();
			}

			object result = null;
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
			return result;
		}

		private bool ShouldUseLongTextStream(ArtifactFieldDTO retrievedField)
		{
			return _toggleProvider.IsEnabled<UseStreamsForBigLongTextFieldsToggle>() && IsLongTextField(retrievedField) &&
				retrievedField.Value?.ToString() == global::Relativity.Constants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN;
		}

		private void LoadNativeFilesLocationsAndNames()
		{
			_readingArtifactIdsReference = ReadingArtifactIDs;
			string documentArtifactIds = string.Join(_separator, ReadingArtifactIDs);
			kCura.Data.DataView dataView = FileQuery.RetrieveNativesForDocuments(Context, documentArtifactIds);

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

		public override bool Read()
		{
			DisposeExtractedTextStreams();
			return base.Read();
		}

		public override void Close()
		{
			DisposeExtractedTextStreams();
			base.Close();
		}

		private void DisposeExtractedTextStreams()
		{
			// Just to be absolutely sure we will not leave any open streams.
			// IAPI should close the streams anyway
			foreach (ILongTextStream stream in _openedStreams)
			{
				stream.Dispose();
			}
			_openedStreams.Clear();
		}

		private static bool IsLongTextField(ArtifactFieldDTO retrievedField)
		{
			return retrievedField.FieldType == FieldTypeHelper.FieldType.Text.ToString() || retrievedField.FieldType == FieldTypeHelper.FieldType.OffTableText.ToString();
		}
	}
}