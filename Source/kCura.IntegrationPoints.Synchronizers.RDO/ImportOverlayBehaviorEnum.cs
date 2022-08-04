namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public enum ImportOverlayBehaviorEnum
    {
        UseRelativityDefaults = kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.UseRelativityDefaults,
        MergeAll = kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.MergeAll,
        ReplaceAll = kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.ReplaceAll
    }
}
