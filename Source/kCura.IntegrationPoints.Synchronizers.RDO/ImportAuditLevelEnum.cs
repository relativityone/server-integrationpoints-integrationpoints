using kCura.EDDS.WebAPI.BulkImportManagerBase;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public enum ImportAuditLevelEnum
    {
        NoAudit = ImportAuditLevel.NoAudit,
        NoSnapshot = ImportAuditLevel.NoSnapshot,
        FullAudit = ImportAuditLevel.FullAudit
    }
}
