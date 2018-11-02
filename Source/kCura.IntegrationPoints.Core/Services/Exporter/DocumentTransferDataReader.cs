﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.EDDS.DocumentCompareGateway;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Toggles;
using FileQuery = Relativity.Core.Service.FileQuery;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DocumentTransferDataReader : ExportTransferDataReaderBase
	{
		/// used as a flag to store the reference of the current artifacts array.
		private object _readingArtifactIdsReference;
		private readonly string[] _textTypeFields = { FieldTypeHelper.FieldType.Text.ToString(), FieldTypeHelper.FieldType.OffTableText.ToString() };

		private readonly Dictionary<int, string> _nativeFileLocations;
		private readonly Dictionary<int, string> _nativeFileNames;
		private readonly Dictionary<int, string> _nativeFileTypes;
		private readonly List<int> _documentsSupportedByViewer;
		private readonly IILongTextStreamFactory _relativityLongTextStreamFactory;
		private readonly List<ILongTextStream> _openedStreams;
		private readonly IAPILog _logger;

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
			IAPILog logger,
			bool useDynamicFolderPath) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories, logger, useDynamicFolderPath)
		{
			_nativeFileLocations = new Dictionary<int, string>();
			_nativeFileNames = new Dictionary<int, string>();
			_nativeFileTypes = new Dictionary<int, string>();
			_documentsSupportedByViewer = new List<int>();
			_relativityLongTextStreamFactory = longTextStreamFactory;
			_openedStreams = new List<ILongTextStream>();
			_logger = logger.ForContext<DocumentTransferDataReader>();
		}


		public override object GetValue(int i)
		{
			bool isFieldIdentifierNumeric = false;
			ArtifactFieldDTO retrievedField = null;
			string fieldIdentifier = "";
			int fieldArtifactId = 0;
			object result = null;
			try
			{
				fieldIdentifier = GetName(i);

				isFieldIdentifierNumeric = int.TryParse(fieldIdentifier, out fieldArtifactId);
				if (isFieldIdentifierNumeric)
				{
					retrievedField = CurrentArtifact.GetFieldForIdentifier(fieldArtifactId);
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
					retrievedField = CurrentArtifact.GetFieldForIdentifier(FolderPathFieldSourceArtifactId);
					return retrievedField.Value;
				}
				if (fieldIdentifier == IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD)
				{
					retrievedField = CurrentArtifact.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME);
					return retrievedField.Value;
				}

				// We have to get native file locations when the reader fetches a new collection of documents.
				if (_readingArtifactIdsReference != ReadingArtifactIDs)
				{
					LoadNativeFilesLocationsAndNames();
					LoadNativesMetadataFromDocumentsTable();
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
					case IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD:
						if (_nativeFileTypes.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileTypes[CurrentArtifact.ArtifactId];
							_nativeFileTypes.Remove(CurrentArtifact.ArtifactId);
						}
						break;
					case IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD:
						if (_documentsSupportedByViewer.Contains(CurrentArtifact.ArtifactId))
						{
							result = true;
							_documentsSupportedByViewer.Remove(CurrentArtifact.ArtifactId);
						}
						else
						{
							result = false;
						}
						break;
				}
				return result;
			}
			catch (Exception e)
			{
				throw LogGetValueError(e, i, isFieldIdentifierNumeric, retrievedField, fieldIdentifier, fieldArtifactId, result);
			}
		}

		private IntegrationPointsException LogGetValueError(
			Exception e, 
			int index, 
			bool isFieldIdentifierNumeric,
			ArtifactFieldDTO retrievedField,
			string fieldIdentifier,
			int fieldArtifactId,
			object result
			)
		{
			string message = $"Error occurred when getting value for index {index}, " +
			              $"isFieldIdentifierNumeric: {isFieldIdentifierNumeric}, " +
			              $"retrievedField: {retrievedField}, " +
			              $"fieldIdentifier: {fieldIdentifier}, " +
			              $"fieldArtifactId: {fieldArtifactId}, " +
			              $"result: {result}";
			string template = "Error occurred when getting value for index {index}, " +
			              "isFieldIdentifierNumeric: {isFieldIdentifierNumeric}, " +
			              "retrievedField: {@retrievedField}, " +
			              "fieldIdentifier: {fieldIdentifier}, " +
			              "fieldArtifactId: {fieldArtifactId}, " +
			              "result: {@result}";
			var exc = new IntegrationPointsException(message, e);
			_logger.LogError(exc, template, index, isFieldIdentifierNumeric, retrievedField, fieldIdentifier, fieldArtifactId, result);
			return exc;
		}

		
		private void LoadNativeFilesLocationsAndNames()
		{
			_readingArtifactIdsReference = ReadingArtifactIDs;
			string documentArtifactIds = string.Join(_separator, ReadingArtifactIDs);
			kCura.Data.DataView dataView = FileQuery.RetrieveNativesForDocuments(Context, documentArtifactIds);

			for (int index = 0; index < dataView.Table.Rows.Count; index++)
			{
				DataRow row = dataView.Table.Rows[index];
				int nativeDocumentArtifactId = (int)row[_nativeDocumentArtifactIdColumn];
				string nativeFileLocation = (string)row[_nativeLocationColumn];
				string nativeFileName = (string)row[_nativeFileNameColumn];
				_nativeFileLocations.Add(nativeDocumentArtifactId, nativeFileLocation);
				_nativeFileNames.Add(nativeDocumentArtifactId, nativeFileName);
				
			}
		}

		private void LoadNativesMetadataFromDocumentsTable()
		{
			string documentArtifactIdColumn = "ArtifactId";
			string supportedByViewerColumn = "SupportedByViewer";
			string relativityNativeTypeColumn = "RelativityNativeType";

			string [] documentColumnsToRetrieve = { supportedByViewerColumn, relativityNativeTypeColumn };

			kCura.Data.DataView nativeTypeForGivenDocument = (kCura.Data.DataView) DocumentQuery.RetrieveValuesByColumnNamesAndArtifactIDs(Context, ReadingArtifactIDs, documentColumnsToRetrieve);

			for (int index = 0; index < nativeTypeForGivenDocument.Table.Rows.Count; index++)
			{
				DataRow row = nativeTypeForGivenDocument.Table.Rows[index];
				var documentArtifactId = (int)row[documentArtifactIdColumn];
				var nativeFileType = (string) row[relativityNativeTypeColumn];
				var isSupportedByViewer = (bool) row[supportedByViewerColumn];

				_nativeFileTypes.Add(documentArtifactId, nativeFileType);

				if (isSupportedByViewer)
				{
					_documentsSupportedByViewer.Add(documentArtifactId);
				}
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
			// IAPI should close the streams...
			// but to be absolutely sure we will not leave any open streams
			// all of them are being disposed here.
			foreach (ILongTextStream stream in _openedStreams)
			{
				stream.Dispose();
			}
			_openedStreams.Clear();
		}

		private bool ShouldUseLongTextStream(ArtifactFieldDTO retrievedField)
		{
			return IsLongTextField(retrievedField) && retrievedField.Value?.ToString() == global::Relativity.Constants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN;
		}

		private bool IsLongTextField(ArtifactFieldDTO retrievedField)
		{
			return retrievedField.FieldType == _textTypeFields[0] || retrievedField.FieldType == _textTypeFields[1];
		}
	}
}