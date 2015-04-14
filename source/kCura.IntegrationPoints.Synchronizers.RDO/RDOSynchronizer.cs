using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.EDDS.WebAPI.DocumentManagerBase;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : kCura.IntegrationPoints.Contracts.Synchronizer.IDataSynchronizer, IBatchReporter
	{
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;

		protected readonly RelativityFieldQuery FieldQuery;
		private Relativity.ImportAPI.IImportAPI _api;
		private readonly ImportApiFactory _factory;
		protected Relativity.ImportAPI.IImportAPI GetImportApi(ImportSettings settings)
		{
			return _api ?? (_api = _factory.GetImportAPI(settings));
		}

		public RdoSynchronizer(RelativityFieldQuery fieldQuery, ImportApiFactory factory)
		{
			FieldQuery = fieldQuery;
			_factory = factory;
		}

		private List<string> IgnoredList
		{
			get
			{
				// fields don't have any space in between words 
				var list = new List<string>
			    {
					"Is System Artifact",
					"System Created By",
					"System Created On",
					"System Last Modified By",
					"System Last Modified On",
					"Artifact ID"
			    };
				return list;
			}
		}

		protected List<Relativity.Client.Artifact> GetRelativityFields(ImportSettings settings)
		{
			var fields = FieldQuery.GetFieldsForRDO(settings.ArtifactTypeId);
			var mappableFields = GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId);
			return fields.Where(x => mappableFields.Any(y => y.ArtifactID == x.ArtifactID)).ToList();
		}

		public virtual IEnumerable<FieldEntry> GetFields(string options)
		{
			ImportSettings settings = GetSettings(options);
			var fields = GetRelativityFields(settings);
			return ParseFields(fields);
		}

		protected IEnumerable<FieldEntry> ParseFields(List<Relativity.Client.Artifact> fields)
		{
			foreach (var result in fields)
			{
				if (!IgnoredList.Contains(result.Name))
				{
					var idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
					bool isIdentifier = false;
					if (idField != null)
					{
						isIdentifier = Convert.ToInt32(idField.Value) == 1;
						if (isIdentifier)
						{
							result.Name += " [Object Identifier]";
						}
					}
					yield return new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString(), IsIdentifier = isIdentifier, IsRequired = false };
				}
			}
		}


		private IImportService _importService;
		private bool _isJobComplete = false;
		private Exception _jobError;
		private List<KeyValuePair<string, string>> _rowErrors;
		private ImportSettings ImportSettings { get; set; }
		private NativeFileImportService NativeFileImportService { get; set; }

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			this.NativeFileImportService = new NativeFileImportService();

			this.ImportSettings = GetSyncDataImportSettings(fieldMap, options, this.NativeFileImportService);

			var importFieldMap = GetSyncDataImportFieldMap(fieldMap, this.ImportSettings);

			_importService = InitializeImportService(this.ImportSettings, importFieldMap, this.NativeFileImportService);

			_isJobComplete = false;
			_jobError = null;
			_rowErrors = new List<KeyValuePair<string, string>>();

			foreach (var row in data)
			{
				var importRow = GenerateImportRow(row, fieldMap, this.ImportSettings);
				if (importRow != null) _importService.AddRow(importRow);
			}
			_importService.PushBatchIfFull(true);

			bool isJobDone = false;
			do
			{
				lock (_importService)
				{
					isJobDone = bool.Parse(_isJobComplete.ToString());
				}
				Thread.Sleep(1000);
			} while (!isJobDone);

			FinalizeSyncData(data, fieldMap, this.ImportSettings);
		}

		private string _webAPIPath;
		public string WebAPIPath
		{
			get
			{
				if (string.IsNullOrEmpty(_webAPIPath))
				{
					_webAPIPath = Config.WebAPIPath;
				}
				return _webAPIPath;
			}
			protected set { _webAPIPath = value; }
		}

		private bool? _disableNativeLocationValidation;
		public bool? DisableNativeLocationValidation
		{
			get
			{
				if (!_disableNativeLocationValidation.HasValue)
				{
					_disableNativeLocationValidation = Config.DisableNativeLocationValidation;
				}
				return _disableNativeLocationValidation;
			}
			protected set { _disableNativeLocationValidation = value; }
		}

		private bool? _disableNativeValidation;
		public bool? DisableNativeValidation
		{
			get
			{
				if (!_disableNativeValidation.HasValue)
				{
					_disableNativeValidation = Config.DisableNativeValidation;
				}
				return _disableNativeValidation;
			}
			protected set { _disableNativeValidation = value; }
		}
		protected virtual ImportService InitializeImportService(ImportSettings settings, Dictionary<string, int> importFieldMap, NativeFileImportService nativeFileImportService)
		{
			ImportService importService = new ImportService(settings, importFieldMap, new BatchManager(), nativeFileImportService, new ImportApiFactory());
			importService.OnBatchComplete += new BatchCompleted(Finish);
			importService.OnDocumentError += new RowError(ItemError);
			importService.OnJobError += new JobError(JobError);

			if (OnBatchComplete != null) importService.OnBatchComplete += OnBatchComplete;
			if (OnBatchSubmit != null) importService.OnBatchSubmit += OnBatchSubmit;
			if (OnBatchCreate != null) importService.OnBatchCreate += OnBatchCreate;
			if (OnJobError != null) importService.OnJobError += OnJobError;
			if (OnDocumentError != null) importService.OnDocumentError += OnDocumentError;

			importService.Initialize();

			return importService;
		}

		protected virtual ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, string options, NativeFileImportService nativeFileImportService)
		{
			ImportSettings settings = GetSettings(options);
			if (string.IsNullOrWhiteSpace(settings.ParentObjectIdSourceFieldName) &&
					fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Parent))
			{
				settings.ParentObjectIdSourceFieldName =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Parent).Select(x => x.SourceField.FieldIdentifier).First();
			}
			if (settings.IdentityFieldId < 1 && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
			{
				settings.IdentityFieldId =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier)
						.Select(x => int.Parse(x.DestinationField.FieldIdentifier))
						.First();
			}
			if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath))
			{
				nativeFileImportService.ImportNativeFiles = true;
				nativeFileImportService.SourceFieldName = fieldMap.First(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath).SourceField.FieldIdentifier;
				settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles;
				settings.DisableNativeLocationValidation = this.DisableNativeLocationValidation;
				settings.DisableNativeValidation = this.DisableNativeValidation;
			}
			return settings;
		}

		protected virtual Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			Dictionary<string, int> importFieldMap = null;

			try
			{
				importFieldMap = fieldMap.Where(x => IncludeFieldInImport(x))
					.ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));
			}
			catch (Exception ex)
			{
				throw new Exception("Field Map is invalid.", ex);
			}
			return importFieldMap;
		}

		protected virtual Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			Dictionary<string, object> importRow = row.ToDictionary(x => x.Key.FieldIdentifier, x => x.Value);
			return importRow;
		}

		protected virtual void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			return;
		}

		protected ImportSettings GetSettings(string options)
		{
			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(options);

			if (string.IsNullOrEmpty(settings.WebServiceURL))
			{
				settings.WebServiceURL = this.WebAPIPath;
				if (string.IsNullOrEmpty(settings.WebServiceURL))
				{
					throw new Exception("No WebAPI path set for integration points.");
				}
			}
			return settings;
		}

		protected bool IncludeFieldInImport(FieldMap fieldMap)
		{
			return (
				fieldMap.FieldMapType != FieldMapTypeEnum.Parent
				&&
				fieldMap.FieldMapType != FieldMapTypeEnum.NativeFilePath
				);
		}

		private void Finish(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			lock (_importService)
			{
				_isJobComplete = true;
			}
		}

		private void JobError(Exception ex)
		{
			lock (_importService)
			{
				_isJobComplete = true;
				_jobError = ex;
			}
		}

		private void ItemError(string documentIdentifier, string errorMessage)
		{
			if (this.ImportSettings.OverwriteMode == OverwriteModeEnum.Overlay
				&& errorMessage.Contains("no document to overwrite"))
			{
				//skip
			}
			else if (this.ImportSettings.OverwriteMode == OverwriteModeEnum.Append
				&& errorMessage.Contains("document with identifier")
				&& errorMessage.Contains("already exists in the workspace"))
			{
				//skip
			}
			else
			{
				_rowErrors.Add(new KeyValuePair<string, string>(documentIdentifier, errorMessage));
			}
		}
	}
}
