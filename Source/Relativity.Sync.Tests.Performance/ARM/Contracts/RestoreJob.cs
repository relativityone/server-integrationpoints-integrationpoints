namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class RestoreJob
	{
		public int JobId { get; set; }
		public string ArchivePath { get; set; }
		public string JobPriority { get; set; }
		public int MatterId { get; set; }
		public int ResourcePoolId { get; set; }
		public int DatabaseServerId { get; set; }
		public int FileRepositoryId { get; set; }
		public int CacheLocationId { get; set; }

		public static ContractEnvelope<RestoreJob> GetRequest(string archivedWorkspacePath)
		{
			return new ContractEnvelope<RestoreJob>
			{
				Contract = new RestoreJob
				{
					JobId = 0,
					ArchivePath = archivedWorkspacePath,
					JobPriority = "Medium",
					MatterId = RelativityConst.RELATIVITY_TEMPLATE_MATTER_ARTIFACT_ID,
					ResourcePoolId = RelativityConst.DEFAULT_RESOURCE_POOL_ID,
					DatabaseServerId = RelativityConst.DATABASE_SERVER_ID,
					FileRepositoryId = RelativityConst.DEFAULT_FILE_REPOSITORY_ID,
					CacheLocationId = RelativityConst.DEFAULT_CACHE_LOCATION_ID
				}
			};
		}
	}
}
