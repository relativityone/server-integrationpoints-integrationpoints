using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public abstract class RdoSynchronizerBase : RdoFieldSynchronizerBase, Contracts.Synchronizer.IDataSynchronizer, IBatchReporter
	{
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;

        private IImportService _importService;
		private bool _isJobComplete = false;
		private Exception _jobError;
		private List<KeyValuePair<string, string>> _rowErrors;
		private ImportSettings ImportSettings { get; set; }
		private NativeFileImportService NativeFileImportService { get; set; }

		protected RdoSynchronizerBase(IRelativityFieldQuery fieldQuery, IImportApiFactory factory) : base(fieldQuery, factory)
		{
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

		public void SyncData(IEnumerable<string> entryIds, IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
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
