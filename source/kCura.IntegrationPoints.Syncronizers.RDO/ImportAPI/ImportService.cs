﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportService : IImportService
	{
		private IImportAPI _importAPI;
		private BatchManager _batchManager;
		private Dictionary<int, Field> _idToFieldDictionary;
		private Dictionary<string, Field> _mappings;
		private Dictionary<string, int> _inputMappings;

		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;


		public ImportService(ImportSettings settings, Dictionary<string, int> fieldMappings, BatchManager batchManager, IImportAPI importAPI = null)
		{
			this.Settings = settings;
			this._batchManager = batchManager;
			this._inputMappings = fieldMappings;
			this._importAPI = importAPI;
			if (_batchManager != null) _batchManager.OnBatchCreate += ImportService_OnBatchCreate;
		}

		public ImportSettings Settings { get; private set; }

		public virtual void Initialize()
		{
			if (_importAPI == null)
			{
				Connect(Settings.WebServiceURL);
				SetupFieldDictionary(_importAPI);
				Dictionary<string, int> fieldMapping = _inputMappings;
				_mappings = ValidateAllMappedFieldsAreInWorkspace(fieldMapping, _idToFieldDictionary);
			}
		}

		public void AddRow(Dictionary<string, object> sourceFields)
		{
			Dictionary<string, object> importFields = GenerateImportFields(sourceFields, FieldMappings);
			_batchManager.Add(importFields);
			PushBatchIfFull(false);
		}

		public bool PushBatchIfFull(bool forcePush)
		{
			bool isFull = _batchManager.IsBatchFull();
			if (isFull || forcePush)
			{
				try
				{
					IDataReader sourceData = _batchManager.GetBatchData();
					this.KickOffImport(sourceData);
				}
				finally
				{
					_batchManager.ClearDataSource();
				}
			}
			return isFull;
		}

		public void CleanUp()
		{
		}

		public virtual void KickOffImport(IDataReader dataReader)
		{
			ImportBulkArtifactJob importJob = _importAPI.NewObjectImportJob(Settings.ArtifactTypeId);
			importJob.SourceData.SourceData = dataReader;

			importJob.Settings.ArtifactTypeId = Settings.ArtifactTypeId;
			importJob.Settings.AuditLevel = Settings.AuditLevel;
			importJob.Settings.CaseArtifactId = Settings.CaseArtifactId;
			importJob.Settings.BulkLoadFileFieldDelimiter = Settings.BulkLoadFileFieldDelimiter;
			importJob.Settings.CopyFilesToDocumentRepository = Settings.CopyFilesToDocumentRepository;
			importJob.Settings.DisableControlNumberCompatibilityMode = Settings.DisableControlNumberCompatibilityMode;
			importJob.Settings.DisableExtractedTextEncodingCheck = Settings.DisableExtractedTextEncodingCheck;
			importJob.Settings.DisableExtractedTextFileLocationValidation = Settings.DisableExtractedTextFileLocationValidation;
			importJob.Settings.DisableNativeLocationValidation = Settings.DisableNativeLocationValidation;
			importJob.Settings.DisableNativeValidation = Settings.DisableNativeValidation;
			importJob.Settings.DisableUserSecurityCheck = Settings.DisableUserSecurityCheck;
			importJob.Settings.ExtractedTextEncoding = Settings.ExtractedTextEncoding;
			importJob.Settings.ExtractedTextFieldContainsFilePath = Settings.ExtractedTextFieldContainsFilePath;
			importJob.Settings.FileSizeColumn = Settings.FileSizeColumn;
			importJob.Settings.FileSizeMapped = Settings.FileSizeMapped;
			importJob.Settings.FolderPathSourceFieldName = Settings.FolderPathSourceFieldName;
			importJob.Settings.IdentityFieldId = Settings.IdentityFieldId;
			importJob.Settings.MaximumErrorCount = Int32.MaxValue - 1; //Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
			importJob.Settings.MultiValueDelimiter = Settings.MultiValueDelimiter;
			importJob.Settings.NativeFileCopyMode = Settings.NativeFileCopyMode;
			importJob.Settings.NativeFilePathSourceFieldName = Settings.NativeFilePathSourceFieldName;
			importJob.Settings.NestedValueDelimiter = Settings.NestedValueDelimiter;
			importJob.Settings.OIFileIdColumnName = Settings.OIFileIdColumnName;
			importJob.Settings.OIFileIdMapped = Settings.OIFileIdMapped;
			importJob.Settings.OIFileTypeColumnName = Settings.OIFileTypeColumnName;
			importJob.Settings.ObjectFieldIdListContainsArtifactId = Settings.ObjectFieldIdListContainsArtifactId;
			importJob.Settings.OverwriteMode = Settings.OverwriteMode;
			importJob.Settings.OverlayBehavior = Settings.OverlayBehavior;
			importJob.Settings.ParentObjectIdSourceFieldName = Settings.ParentObjectIdSourceFieldName;
			importJob.Settings.SendEmailOnLoadCompletion = Settings.SendEmailOnLoadCompletion;
			importJob.Settings.StartRecordNumber = Settings.StartRecordNumber;
			importJob.Settings.SelectedIdentifierFieldName = _idToFieldDictionary[Settings.IdentityFieldId].Name;

			importJob.OnComplete += new IImportNotifier.OnCompleteEventHandler(ImportJob_OnComplete);
			ImportService_OnBatchSubmit(_batchManager.CurrentSize, _batchManager.MinimumBatchSize);

			importJob.Execute();
		}

		private Dictionary<string, Field> FieldMappings
		{
			get { return _mappings; }
		}

		internal void Connect(string url)
		{
			try
			{
				_importAPI = new ExtendedImportAPI(url);
			}
			catch (Exception ex)
			{
				//LoggedException.PreserveStack(ex);
				throw;
			}
		}

		internal void SetupFieldDictionary(IImportAPI api)
		{
			try
			{
				_idToFieldDictionary = new Dictionary<int, Field>();

				var workspaceFields = api.GetWorkspaceFields(Settings.CaseArtifactId, Settings.ArtifactTypeId);
				foreach (var field in workspaceFields)
				{
					_idToFieldDictionary.Add(field.ArtifactID, field);
				}
			}
			catch (Exception ex)
			{
				//LoggedException.PreserveStack(ex);
				//throw new ConnectionException(RelativityExport.RelativityWorkspaceRead, ex);
				throw;
			}
		}

		public Dictionary<string, Field> ValidateAllMappedFieldsAreInWorkspace(Dictionary<string, int> fieldMapping, Dictionary<int, Field> rdoAllFields)
		{
			Dictionary<string, Field> mapping = new Dictionary<string, Field>();

			List<int> missingFields = new List<int>();
			foreach (string mapSourceFieldName in fieldMapping.Keys)
			{
				int mapRDOFieldID = fieldMapping[mapSourceFieldName];
				if (!rdoAllFields.ContainsKey(mapRDOFieldID))
				{
					missingFields.Add(mapRDOFieldID);
				}
				else
				{
					if (!mapping.ContainsKey(mapSourceFieldName))
						mapping.Add(mapSourceFieldName, rdoAllFields[mapRDOFieldID]);
				}
			}
			if (missingFields.Count > 0)
			{
				string missingFieldFormatted = String.Join(", ", missingFields);
				throw new Exception(string.Format("Missing mapped field IDs: {0}", missingFieldFormatted));
			}
			return mapping;
		}

		public Dictionary<string, object> GenerateImportFields(Dictionary<string, object> sourceFields, Dictionary<string, Field> mapping)
		{
			Dictionary<string, object> importFields = new Dictionary<string, object>();

			foreach (string sourceFieldID in sourceFields.Keys)
			{
				if (mapping.ContainsKey(sourceFieldID))
				{
					Field rdoField = mapping[sourceFieldID];

					if (!importFields.ContainsKey(rdoField.Name))
						importFields.Add(rdoField.Name, sourceFields[sourceFieldID]);
				}
			}
			return importFields;
		}

		private void ImportService_OnBatchCreate(int batchSize)
		{
			if (OnBatchCreate != null)
			{
				OnBatchCreate(batchSize);
			}
		}

		private void ImportService_OnBatchSubmit(int currentSize, int minSize)
		{
			if (OnBatchSubmit != null)
			{
				OnBatchSubmit(currentSize, minSize);
			}
		}

		private void ImportJob_OnComplete(JobReport jobReport)
		{
			ImportJob_OnDocumentError(jobReport.ErrorRows);

			if (jobReport.FatalException != null)
			{
				ImportJob_OnError(jobReport.FatalException);
			}
			else
			{
				CompleteBatch(jobReport.StartTime, jobReport.EndTime, jobReport.TotalRows, jobReport.ErrorRowCount);
			}
		}

		private void ImportJob_OnError(Exception fatalException)
		{
			if (OnJobError != null)
			{
				OnJobError(fatalException);
			}
		}

		private void ImportJob_OnDocumentError(IList<JobReport.RowError> errors)
		{
			if (OnDocumentError != null)
			{
				foreach (JobReport.RowError error in errors)
				{
					OnDocumentError(error.Identifier, error.Message);
				}
			}
		}

		private void CompleteBatch(DateTime start, DateTime end, int totalRows, int errorRows)
		{
			if (OnBatchComplete != null)
			{
				OnBatchComplete(start, end, totalRows, errorRows);
			}
		}

	}
}
