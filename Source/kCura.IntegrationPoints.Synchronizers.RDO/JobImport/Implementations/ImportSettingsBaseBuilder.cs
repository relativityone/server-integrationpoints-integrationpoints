using System;
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
            target.DisableUserSecurityCheck = false;
            target.ExtractedTextFieldContainsFilePath = importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath;
            target.IdentityFieldId = importSettings.DestinationConfiguration.IdentityFieldId;
            target.MaximumErrorCount = int.MaxValue - 1; // Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
            target.NativeFileCopyMode = importSettings.NativeFileCopyMode;
            target.ObjectFieldIdListContainsArtifactId = importSettings.ObjectFieldIdListContainsArtifactId;
            target.OverwriteMode = ConvertToOverwriteModeEnum(importSettings.DestinationConfiguration.ImportOverwriteMode);
            target.OverlayBehavior = importSettings.ImportOverlayBehavior;
            target.ParentObjectIdSourceFieldName = importSettings.ParentObjectIdSourceFieldName;
            target.SendEmailOnLoadCompletion = false;
            target.StartRecordNumber = importSettings.StartRecordNumber;
            target.Billable = importSettings.DestinationConfiguration.CopyFilesToDocumentRepository; // mark files as billable only when copying
        }

        private OverwriteModeEnum ConvertToOverwriteModeEnum(ImportOverwriteModeEnum importOverwriteMode)
        {
            switch (importOverwriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly: return OverwriteModeEnum.Append;
                case ImportOverwriteModeEnum.AppendOverlay: return OverwriteModeEnum.AppendOverlay;
                case ImportOverwriteModeEnum.OverlayOnly: return OverwriteModeEnum.Overlay;
                default: throw new NotSupportedException($"Unknown value of {nameof(ImportOverwriteModeEnum)}: {importOverwriteMode}");
            }
        }
    }
}
