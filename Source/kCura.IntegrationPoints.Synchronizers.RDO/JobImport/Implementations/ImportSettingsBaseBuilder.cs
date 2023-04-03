using kCura.EDDS.WebAPI.BulkImportManagerBase;
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
            target.AuditLevel = ImportAuditLevel.FullAudit;
            target.CaseArtifactId = importSettings.DestinationConfiguration.CaseArtifactId;
            target.CopyFilesToDocumentRepository = importSettings.DestinationConfiguration.CopyFilesToDocumentRepository;
            target.DisableExtractedTextEncodingCheck = false;
            target.DisableUserSecurityCheck = false;
            target.ExtractedTextFieldContainsFilePath = importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath;
            target.IdentityFieldId = importSettings.DestinationConfiguration.IdentityFieldId;
            target.MaximumErrorCount = int.MaxValue - 1; // Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
            target.NativeFileCopyMode = importSettings.NativeFileCopyMode;
            target.ObjectFieldIdListContainsArtifactId = importSettings.ObjectFieldIdListContainsArtifactId;
            target.OverwriteMode = (OverwriteModeEnum)importSettings.DestinationConfiguration.ImportOverwriteMode;
            target.OverlayBehavior = importSettings.ImportOverlayBehavior;
            target.ParentObjectIdSourceFieldName = importSettings.ParentObjectIdSourceFieldName;
            target.SendEmailOnLoadCompletion = false;
            target.StartRecordNumber = importSettings.StartRecordNumber;
            target.Billable = importSettings.DestinationConfiguration.CopyFilesToDocumentRepository; // mark files as billable only when copying
        }
    }
}
