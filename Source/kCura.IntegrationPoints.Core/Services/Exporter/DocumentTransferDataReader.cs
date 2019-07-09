﻿using System;
using Stream = System.IO.Stream;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Services.Objects.DataContracts;
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
		private readonly Dictionary<int, long> _nativeFileSizes;
		private readonly Dictionary<int, string> _nativeFileTypes;
		private readonly HashSet<int> _documentsSupportedByViewer;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly IQueryFieldLookupRepository _fieldLookupRepository;
		private readonly IAPILog _logger;

		private const string _DUPLICATED_NATIVE_KEY_ERROR_MESSAGE = "Duplicated key found.Check if there is no natives duplicates for a given document";

		private static readonly string _nativeDocumentArtifactIdColumn = "DocumentArtifactID";
		private static readonly string _nativeFileNameColumn = "Filename";
		private static readonly string _nativeFileSizeColumn = "Size";
		private static readonly string _nativeLocationColumn = "Location";
		private static readonly string _separator = ",";

		public DocumentTransferDataReader(IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories,
			IRelativityObjectManager relativityObjectManager,
			IAPILog logger,
			IQueryFieldLookupRepository fieldLookupRepository,
			bool useDynamicFolderPath) :
			base(relativityExportService, fieldMappings, context, scratchTableRepositories, logger, useDynamicFolderPath)
		{
			_nativeFileLocations = new Dictionary<int, string>();
			_nativeFileNames = new Dictionary<int, string>();
			_nativeFileSizes = new Dictionary<int, long>();
			_nativeFileTypes = new Dictionary<int, string>();
			_documentsSupportedByViewer = new HashSet<int>();
			_relativityObjectManager = relativityObjectManager;
			_fieldLookupRepository = fieldLookupRepository;
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
						return GetLongTextStreamFromField(fieldArtifactId);
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
					case IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD:
						if (_nativeFileSizes.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileSizes[CurrentArtifact.ArtifactId];
							_nativeFileSizes.Remove(CurrentArtifact.ArtifactId);
						}
						else
						{
							result = false;
						}
						break;
					case IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD:
						if (_nativeFileTypes.ContainsKey(CurrentArtifact.ArtifactId))
						{
							result = _nativeFileTypes[CurrentArtifact.ArtifactId];
							_nativeFileTypes.Remove(CurrentArtifact.ArtifactId);
						}
						else
						{
							result = false;
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

		private Stream GetLongTextStreamFromField(int fieldArtifactId)
		{
			var fieldRef = new FieldRef { ArtifactID = fieldArtifactId };
			ViewFieldInfo field = _fieldLookupRepository.GetFieldByArtifactId(fieldArtifactId);

			return field.IsUnicodeEnabled
				? _relativityObjectManager.StreamUnicodeLongText(CurrentArtifact.ArtifactId, fieldRef)
				: _relativityObjectManager.StreamNonUnicodeLongText(CurrentArtifact.ArtifactId, fieldRef);
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
				int nativeDocumentArtifactId = (int) row[_nativeDocumentArtifactIdColumn];
				string nativeFileLocation = (string) row[_nativeLocationColumn];
				_nativeFileLocations.AddAndFailJobIfKeyExists(
					nativeDocumentArtifactId, 
					nativeFileLocation,
					_DUPLICATED_NATIVE_KEY_ERROR_MESSAGE,
					_logger
				);
				string nativeFileName = (string)row[_nativeFileNameColumn];
				_nativeFileNames.AddAndFailJobIfKeyExists(
					nativeDocumentArtifactId, 
					nativeFileName,
					_DUPLICATED_NATIVE_KEY_ERROR_MESSAGE,
					_logger
					
				);
				long nativeFileSize = (long)row[_nativeFileSizeColumn];
				_nativeFileSizes.AddAndFailJobIfKeyExists(
					nativeDocumentArtifactId, 
					nativeFileSize,
					_DUPLICATED_NATIVE_KEY_ERROR_MESSAGE,
					_logger
				);
			}
		}

		private void LoadNativesMetadataFromDocumentsTable()
		{
			string documentArtifactIdColumn = "ArtifactId";
			string supportedByViewerColumn = "SupportedByViewer";
			string relativityNativeTypeColumn = "RelativityNativeType";

			string[] documentColumnsToRetrieve = { supportedByViewerColumn, relativityNativeTypeColumn };

			kCura.Data.DataView nativeTypeForGivenDocument = DocumentQuery.RetrieveValuesByColumnNamesAndArtifactIDs(Context, ReadingArtifactIDs, documentColumnsToRetrieve);

			for (int index = 0; index < nativeTypeForGivenDocument.Table.Rows.Count; index++)
			{
				DataRow row = nativeTypeForGivenDocument.Table.Rows[index];
				var documentArtifactId = (int)row[documentArtifactIdColumn];
				string nativeFileType = Convert.ToString(row[relativityNativeTypeColumn]);

				if (!string.IsNullOrEmpty(nativeFileType))
				{
					_nativeFileTypes.Add(documentArtifactId, nativeFileType);
				}

				bool supportedByViewer;
				if (bool.TryParse(row[supportedByViewerColumn].ToString(), out supportedByViewer) && supportedByViewer)
				{
					_documentsSupportedByViewer.Add(documentArtifactId);
				}
			}
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