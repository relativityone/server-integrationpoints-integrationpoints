using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public class ImportSettingsBaseBuilder<T> : IImportSettingsBaseBuilder<T> where T : ImportSettingsBase
    {
        protected IImportAPI _importApi;
        protected ImportSettingsBaseBuilder(IImportAPI importApi)
        {
            _importApi = importApi;
        }

        public virtual void PopulateFrom(ImportSettings importSettings, T target)
        {
            target.AuditLevel = importSettings.AuditLevel;
            target.CaseArtifactId = importSettings.CaseArtifactId;
            target.CopyFilesToDocumentRepository = importSettings.CopyFilesToDocumentRepository;
            target.DisableExtractedTextEncodingCheck = importSettings.DisableExtractedTextEncodingCheck;
            target.DisableUserSecurityCheck = importSettings.DisableUserSecurityCheck;
            target.ExtractedTextFieldContainsFilePath = importSettings.ExtractedTextFieldContainsFilePath;
            target.IdentityFieldId = importSettings.IdentityFieldId;
            target.MaximumErrorCount = int.MaxValue - 1; //Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
            target.NativeFileCopyMode = importSettings.NativeFileCopyMode;
            target.ObjectFieldIdListContainsArtifactId = importSettings.ObjectFieldIdListContainsArtifactId;
            target.OverwriteMode = importSettings.OverwriteMode;
            target.OverlayBehavior = importSettings.OverlayBehavior;
            target.ParentObjectIdSourceFieldName = importSettings.ParentObjectIdSourceFieldName;
            target.SendEmailOnLoadCompletion = importSettings.SendEmailOnLoadCompletion;
            target.StartRecordNumber = importSettings.StartRecordNumber;
            target.Billable = importSettings.CopyFilesToDocumentRepository; // mark files as billable only when copying
        }
    }
}
