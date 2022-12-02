using System.Collections.Generic;
using System.Text;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Logging
{
    internal abstract class ImportSettingsForLoggingBase
    {
        protected const string _REMOVED_SENSITIVE_DATA = "[Sensitive data has been removed]";

        protected ImportSettingsForLoggingBase(ImportSettingsBase settings)
        {
            AuditLevel = settings.AuditLevel;
            CaseArtifactId = settings.CaseArtifactId;
            CopyFilesToDocumentRepository = settings.CopyFilesToDocumentRepository;
            DestinationFolderArtifactID = settings.DestinationFolderArtifactID;
            DisableExtractedTextEncodingCheck = settings.DisableExtractedTextEncodingCheck;
            DisableUserSecurityCheck = settings.DisableUserSecurityCheck;
            ExtractedTextEncoding = settings.ExtractedTextEncoding;
            ExtractedTextFieldContainsFilePath = settings.ExtractedTextFieldContainsFilePath;
            LoadImportedFullTextFromServer = settings.LoadImportedFullTextFromServer;
            IdentityFieldId = settings.IdentityFieldId;
            MaximumErrorCount = settings.MaximumErrorCount;
            NativeFileCopyMode = settings.NativeFileCopyMode;
            OverwriteMode = settings.OverwriteMode;
            SendEmailOnLoadCompletion = settings.SendEmailOnLoadCompletion;
            WebServiceURL = settings.WebServiceURL;
            StartRecordNumber = settings.StartRecordNumber;
            ObjectFieldIdListContainsArtifactId = settings.ObjectFieldIdListContainsArtifactId;
            OverlayBehavior = settings.OverlayBehavior;
            MoveDocumentsInAppendOverlayMode = settings.MoveDocumentsInAppendOverlayMode;
            Billable = settings.Billable;
            ApplicationName = settings.ApplicationName;

            DataGridIDColumnName = RemoveSensitiveDataIfNotEmpty(settings.DataGridIDColumnName);
            ParentObjectIdSourceFieldName = RemoveSensitiveDataIfNotEmpty(settings.ParentObjectIdSourceFieldName);
            RelativityPassword = RemoveSensitiveDataIfNotEmpty(settings.RelativityPassword);
            RelativityUsername = RemoveSensitiveDataIfNotEmpty(settings.RelativityUsername);
            SelectedIdentifierFieldName = RemoveSensitiveDataIfNotEmpty(settings.SelectedIdentifierFieldName);
        }

        #region Properties

        public kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel { get; set; }

        public int CaseArtifactId { get; set; }

        public bool CopyFilesToDocumentRepository { get; set; }

        public string DataGridIDColumnName { get; set; }

        public int DestinationFolderArtifactID { get; set; }

        public bool? DisableExtractedTextEncodingCheck { get; set; }

        public bool DisableUserSecurityCheck { get; set; }

        public Encoding ExtractedTextEncoding { get; set; }

        public bool ExtractedTextFieldContainsFilePath { get; set; }

        public bool LoadImportedFullTextFromServer { get; set; }

        public int IdentityFieldId { get; set; }

        public int? MaximumErrorCount { get; set; }

        public NativeFileCopyModeEnum NativeFileCopyMode { get; set; }

        public OverwriteModeEnum OverwriteMode { get; set; }

        public string ParentObjectIdSourceFieldName { get; set; }

        public string RelativityPassword { get; set; }

        public string RelativityUsername { get; set; }

        public string SelectedIdentifierFieldName { get; set; }

        public bool SendEmailOnLoadCompletion { get; set; }

        public string WebServiceURL { get; set; }

        public long StartRecordNumber { get; set; }

        public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }

        public OverlayBehavior OverlayBehavior { get; set; }

        public bool MoveDocumentsInAppendOverlayMode { get; set; }

        public bool Billable { get; set; }

        public string ApplicationName { get; set; }

        #endregion

        protected string RemoveSensitiveDataIfNotEmpty(string setting)
        {
            if (setting is null)
            {
                return null;
            }

            return "[Sensitive data has been removed]";
        }
    }
}
