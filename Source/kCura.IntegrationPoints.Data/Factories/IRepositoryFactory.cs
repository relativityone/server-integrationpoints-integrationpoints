using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Factories
{
	/// <summary>
	/// Responsible for creating the necessary repository classes.
	/// </summary>
	public interface IRepositoryFactory
	{
		/// <summary>
		/// Returns a class implementing the IArtifactGuidRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IArtifactGuidRepository interface.</returns>
		IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ICodeRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ICodeRepository interface</returns>
		ICodeRepository GetCodeRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IDestinationProviderRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IDestinationProviderRepository interface</returns>
		IDestinationProviderRepository GetDestinationProviderRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IDestinationWorkspaceRepository interface.
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The source workspace artifact id.</param>
		/// <returns>A class implementing the IDestinationWorkspaceRepository interface.</returns>
		IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IDocumentRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IDocumentRepository interface.</returns>
		IDocumentRepository GetDocumentRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IFieldQueryRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IFieldQueryRepository interface.</returns>
		IFieldQueryRepository GetFieldQueryRepository(int workspaceArtifactId);
		
		IFieldRepository GetFieldRepository(int workspaceArtifactId);
		
		/// <summary>
		/// Returns a class implementing the IJobHistoryRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IJobHistoryRepository interface.</returns>
		IJobHistoryRepository GetJobHistoryRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IJobHistoryErrorRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the IJobHistoryErrorRepository interface.</returns>
		IJobHistoryErrorRepository GetJobHistoryErrorRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IObjectRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <param name="rdoArtifactId">The artifact type id of the relativity object</param>
		/// <returns>A class implementing the IObjectRepository interface</returns>
		IObjectRepository GetObjectRepository(int workspaceArtifactId, int rdoArtifactId);

		/// <summary>
		/// Returns a class implementing the IObjectTypeRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IObjectTypeRepository interface.</returns>
		IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId);

		IObjectTypeRepository GetDestinationObjectTypeRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IPermissionRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the IPermissionRepository interface.</returns>
		IPermissionRepository GetPermissionRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IQueueRepository interface.
		/// </summary>
		/// <returns>A class implementing the IQueueRepository interface</returns>
		IQueueRepository GetQueueRepository();

		/// <summary>
		/// Returns a class implementing the IScratchTableRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactID">Artifact Id of the source workspace.</param>
		/// <param name="tablePrefix">Prefix for any temp table that will be handled by this repository</param>
		/// <param name="tableSuffix">Suffix for any temp table that will be handled by this repository</param>
		/// <returns>A class implementing the IScratchTableRepository interface</returns>
		IScratchTableRepository GetScratchTableRepository(int workspaceArtifactID, string tablePrefix, string tableSuffix);

		/// <summary>
		/// Returns a class implementing the ISourceJobRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the ISourceJobRepository interface.</returns>
		ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId);
		
		/// <summary>
		/// Returns a class implementing the ISourceProviderRepository interface
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id</param>
		/// <returns>A class implementing the ISourceProviderRepository interface</returns>
		ISourceProviderRepository GetSourceProviderRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ISourceWorkspaceRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the ISourceWorkspaceRepository interface.</returns>
		ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ITabRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns>A class implementing the ITabRepository interface.</returns>
		ITabRepository GetTabRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IWorkspaceRepository interface.
		/// </summary>
		/// <returns>A class implementing the IWorkspaceRepository interface.</returns>
		IWorkspaceRepository GetWorkspaceRepository();

		IWorkspaceRepository GetSourceWorkspaceRepository();

		/// <summary>
		/// Returns a class implementing the IErrorRepository interface.
		/// </summary>
		/// <returns>A class implementing the IErrorRepository interface.</returns>
		IErrorRepository GetErrorRepository();

		/// <summary>
		/// Returns a class implementing the ISavedSearchRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id.</param>
		/// <param name="savedSearchArtifactId">Saved search artifact id.</param>
		/// <returns>A class implementing the ISavedSearchRepository interface.</returns>
		ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId);

		/// <summary>
		/// Returns a class implementing the IProductionRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id.</param>
		/// <returns>A class implementing the IProductionRepository interface.</returns>
		IProductionRepository GetProductionRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the ISavedSearchQueryRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id.</param>
		/// <returns>A class implementing the ISavedSearchQueryRepository interface.</returns>
		ISavedSearchQueryRepository GetSavedSearchQueryRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IInstanceSettingRepository interface.
		/// </summary>
		/// <returns>A class implementing the IInstanceSettingRepository interface.</returns>
		IInstanceSettingRepository GetInstanceSettingRepository();

		/// <summary>
		/// Returns a class implementing the IRelativityAuditRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id.</param>
		/// <returns>A class implementing the IRelativityAuditRepository interface.</returns>
		IRelativityAuditRepository GetRelativityAuditRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IResourcePoolRepository interface.
		/// </summary>
		/// <returns>A class implementing the IResourcePoolRepository interface.</returns>
		IResourcePoolRepository GetResourcePoolRepository();
		
		IKeywordSearchRepository GetKeywordSearchRepository();

        /// <summary>
        /// Returns a class implementing the IQueryFieldLookupRepository interface.
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <returns>A class implementing the IQueryFieldLookupRepository interface.</returns>
	    IQueryFieldLookupRepository GetQueryFieldLookupRepository(int workspaceArtifactId);

		/// <summary>
		/// Returns a class implementing the IAuditRepository interface.
		/// </summary>
		/// <param name="workspaceArtifactId"></param>
		/// <returns>A class implementing the IAuditRepository interface.</returns>
		IAuditRepository GetAuditRepository(int workspaceArtifactId);

		ICaseRepository GetCaseRepository();
    }
}