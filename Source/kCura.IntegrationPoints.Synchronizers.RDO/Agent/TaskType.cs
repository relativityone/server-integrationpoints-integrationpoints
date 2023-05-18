namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    /// <summary>
    /// TaskType is responsible for determine job type which, runs in Agent
    /// <para><see cref="TaskType.None"/>: Default initial type used for testing. Unused anywhere</para>
    /// <para><see cref="TaskType.SyncManager"/>: Used in Custom Providers (FTP, LDAP, Others). Manage the job (create sub-batches)</para>
    /// <para><see cref="TaskType.SyncWorker"/>: Process import batch defined by <see cref="TaskType.SyncManager"/></para>
    /// <para><see cref="TaskType.SyncEntityManagerWorker"/>: Used for import when Entity has been selected. First jobs are registered by <see cref="TaskType.SyncManager"/>, but after that go with different path </para>
    /// <para><see cref="TaskType.SendEmailManager"/>/<see cref="TaskType.SendEmailWorker"/>: Used for sending notifications e-mails when job finished</para>
    /// <para><see cref="TaskType.ExportService"/>: Pushing data between Relativity (Images->Folder; Production->Folder/Production)</para>
    /// <para><see cref="TaskType.ExportManager"/>: Manage Export job (create sub-batches). Export Relativity types to Load File</para>
    /// <para><see cref="TaskType.ExportWorker"/>: Process export batch defined by <see cref="TaskType.ExportManager"/></para>
    /// <para><see cref="TaskType.ImportService"/>: Import Load File to Relativity</para>
    /// </summary>
    public enum TaskType
    {
        None,
        SyncManager,
        SyncWorker,
        SyncEntityManagerWorker,
        SendEmailManager,
        SendEmailWorker,
        ExportService,
        ExportManager,
        ExportWorker,
        ImportService
    }
}
