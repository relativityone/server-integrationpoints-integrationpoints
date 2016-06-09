using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Newtonsoft.Json;
using Artifact = kCura.Relativity.Client.Artifact;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public abstract class RdoSynchronizerBase : Contracts.Synchronizer.IDataSynchronizer, IBatchReporter, IEmailBodyData
    {
        public event BatchCompleted OnBatchComplete;

        public event BatchSubmitted OnBatchSubmit;

        public event BatchCreated OnBatchCreate;

        public event StatusUpdate OnStatusUpdate;

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

        private HashSet<string> _ignoredList;

        private HashSet<string> IgnoredList
        {
            get
            {
                // fields don't have any space in between words
                if (_ignoredList == null)
                {
                    _ignoredList = new HashSet<string>
          {
            "Is System Artifact",
            "System Created By",
            "System Created On",
            "System Last Modified By",
            "System Last Modified On",
            "Artifact ID"
          };
                }
                return _ignoredList;
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
                    _webAPIPath = kCura.IntegrationPoints.Config.Config.Instance.WebApiPath;
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
                    _disableNativeLocationValidation = kCura.IntegrationPoints.Config.Config.Instance.DisableNativeLocationValidation;
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
                    _disableNativeValidation = kCura.IntegrationPoints.Config.Config.Instance.DisableNativeValidation;
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

            if (OnBatchComplete != null)
            {
                importService.OnBatchComplete += OnBatchComplete;
            }
            if (OnBatchSubmit != null)
            {
                importService.OnBatchSubmit += OnBatchSubmit;
            }
            if (OnBatchCreate != null)
            {
                importService.OnBatchCreate += OnBatchCreate;
            }
            if (OnStatusUpdate != null)
            {
                importService.OnStatusUpdate += OnStatusUpdate;
            }
            if (OnJobError != null)
            {
                importService.OnJobError += OnJobError;
            }
            if (OnDocumentError != null)
            {
                importService.OnDocumentError += OnDocumentError;
            }
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

            if (settings.ImportNativeFile)
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
                // NOTE :: If you expect to import the folder path, the import API will expect this field to be specified upon import. This is to avoid the field being both mapped and used as a folder path.
                settings.FolderPathSourceFieldName = Contracts.Constants.SPECIAL_FOLDERPATH_FIELD_NAME;
            }

            if (SourceProvider != null && SourceProvider.Config.AlwaysImportNativeFileNames)
            {
                // So that the destination workspace file icons correctly display, we give the import API the file name of the document
                settings.FileNameColumn = Contracts.Constants.SPECIAL_FILE_NAME_FIELD_NAME;
            }

            if (SourceProvider != null && SourceProvider.Config.OnlyMapIdentifierToIdentifier)
            {
                FieldMap map = fieldMap.First(field => field.FieldMapType == FieldMapTypeEnum.Identifier);
                if (!(map.SourceField.IsIdentifier && map.DestinationField.IsIdentifier))
                {
                    throw new Exception("Source Provider requires the identifier field to be mapped with another identifier field.");
                }
            }

            return settings;
        }

        protected virtual Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
        {
            Dictionary<string, int> importFieldMap = null;
            try
            {
                importFieldMap = fieldMap.Where(IncludeFieldInImport).ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));
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
            bool toInclude = fieldMap.FieldMapType != FieldMapTypeEnum.Parent &&
                             fieldMap.FieldMapType != FieldMapTypeEnum.NativeFilePath;
            if (toInclude && fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
            {
                toInclude = fieldMap.DestinationField != null && fieldMap.DestinationField.FieldIdentifier != null;
            }
            return toInclude;
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

        public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
        {
            ImportSettings settings = GetSettings(options);
            WorkspaceRef destinationWorkspace = GetWorkspace(settings);

            StringBuilder emailBody = new StringBuilder();
            if (destinationWorkspace != null)
            {
                emailBody.AppendLine("");
                emailBody.AppendFormat("Destination Workspace: {0}", Utils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.Name, destinationWorkspace.Id));
            }
            return emailBody.ToString();
        }

        protected virtual WorkspaceRef GetWorkspace(ImportSettings settings)
        {
            WorkspaceRef workspaceRef = null;
            Workspace workspace = GetImportApi(settings).Workspaces().FirstOrDefault(x => x.ArtifactID == settings.CaseArtifactId);
            if (workspace != null)
            {
                workspaceRef = new WorkspaceRef() { Id = workspace.ArtifactID, Name = workspace.Name };
            }
            return workspaceRef;
        }
    }
}