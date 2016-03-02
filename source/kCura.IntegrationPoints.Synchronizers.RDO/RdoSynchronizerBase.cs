using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public abstract class RdoSynchronizerBase : Contracts.Synchronizer.IDataSynchronizer, IBatchReporter
	{
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;

		protected readonly IRelativityFieldQuery FieldQuery;
		private Relativity.ImportAPI.IImportAPI _api;
		private readonly IImportApiFactory _factory;

		private IImportService _importService;
		private bool _isJobComplete = false;
		private Exception _jobError;
		private List<KeyValuePair<string, string>> _rowErrors;
		private ImportSettings ImportSettings { get; set; }
		private NativeFileImportService NativeFileImportService { get; set; }

		public SourceProvider SourceProvider { get; set; }

		protected RdoSynchronizerBase(IRelativityFieldQuery fieldQuery, IImportApiFactory factory)
		{
			FieldQuery = fieldQuery;
			_factory = factory;
		}

		protected Relativity.ImportAPI.IImportAPI GetImportApi(ImportSettings settings)
		{
			return _api ?? (_api = _factory.GetImportAPI(settings));
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
			List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
			return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
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

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			IntializeImportJob(fieldMap, options);

			bool movedNext = true;
			IEnumerator<IDictionary<FieldEntry, object>> enumerator = data.GetEnumerator();

			do
			{
				try
				{
					movedNext = enumerator.MoveNext();
					if (movedNext)
					{
						var importRow = GenerateImportRow(enumerator.Current, fieldMap, this.ImportSettings);
						if (importRow != null)
						{
							_importService.AddRow(importRow);
						}
					}
				}
				catch (ProviderReadDataException exception)
				{
					ItemError(exception.Identifier, exception.Message);
				}

			} while (movedNext);


			_importService.PushBatchIfFull(true);

			WaitUntilTheJobIsDone();

			FinalizeSyncData(data, fieldMap, this.ImportSettings);
		}

		public void SyncData(IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
		{
			IntializeImportJob(fieldMap, options);

			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
			IDataReader sourceReader = new RelativityReaderDecorator(data, fieldMaps);

			_importService.KickOffImport(sourceReader);

			WaitUntilTheJobIsDone();
		}


		private void IntializeImportJob(IEnumerable<FieldMap> fieldMap, string options)
		{
			this.NativeFileImportService = new NativeFileImportService();

			this.ImportSettings = GetSyncDataImportSettings(fieldMap, options, this.NativeFileImportService);

			var importFieldMap = GetSyncDataImportFieldMap(fieldMap, this.ImportSettings);

			_importService = InitializeImportService(this.ImportSettings, importFieldMap, this.NativeFileImportService);

			_isJobComplete = false;
			_jobError = null;
			_rowErrors = new List<KeyValuePair<string, string>>();
		}

		private void WaitUntilTheJobIsDone()
		{
			bool isJobDone = false;
			do
			{
				lock (_importService)
				{
					isJobDone = bool.Parse(_isJobComplete.ToString());
				}
				Thread.Sleep(1000);
			} while (!isJobDone);
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

			if (settings.ImportNativeFiles)
			{
				nativeFileImportService.ImportNativeFiles = true;
				FieldMap field = fieldMap.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath);
				nativeFileImportService.SourceFieldName = field != null ? field.SourceField.FieldIdentifier : Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
				settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.DisableNativeLocationValidation = this.DisableNativeLocationValidation;
				settings.DisableNativeValidation = this.DisableNativeValidation;
				settings.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles;
			}
			else if (SourceProvider != null && SourceProvider.Config.AlwaysImportNativeFiles)
			{
				nativeFileImportService.ImportNativeFiles = true;
				settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.DisableNativeLocationValidation = this.DisableNativeLocationValidation;
				settings.DisableNativeValidation = this.DisableNativeValidation;
				settings.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks;
				nativeFileImportService.SourceFieldName = Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
			}

			if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation))
			{
				settings.FolderPathSourceFieldName = fieldMap.First(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation).SourceField.ActualName;
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
				&&
				fieldMap.FieldMapType != FieldMapTypeEnum.FolderPathInformation
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
			_rowErrors.Add(new KeyValuePair<string, string>(documentIdentifier, errorMessage));
		}
	}
}
