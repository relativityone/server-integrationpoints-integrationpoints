namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class RestoreJob
	{
		private const int _RELATIVITY_TEMPLATE_MATTER_ARTIFACT_ID = 1000002;
		private const int _DEFAULT_FILE_REPOSITORY_ID = 1014887;
		private const int _DATABASE_SERVER_ID = 1017224;
		private const int _DEFAULT_CACHE_LOCATION_ID = 1015534;

		public int JobId { get; set; }
		public string ArchivePath { get; set; }
		public string JobPriority { get; set; }
		public int MatterId { get; set; }
		public int ResourcePoolId { get; set; }
		public int DatabaseServerId { get; set; }
		public int FileRepositoryId { get; set; }
		public int CacheLocationId { get; set; }

		public static ContractEnvelope<RestoreJob> GetRequest(string archivedWorkspacePath, int resourcePoolId)
		{
			return new ContractEnvelope<RestoreJob>
			{
				Contract = new RestoreJob
				{
					JobId = 0,
					ArchivePath = archivedWorkspacePath,
					JobPriority = "Medium",
					MatterId = _RELATIVITY_TEMPLATE_MATTER_ARTIFACT_ID,
					ResourcePoolId = resourcePoolId,
					DatabaseServerId = _DATABASE_SERVER_ID,
					FileRepositoryId = _DEFAULT_FILE_REPOSITORY_ID,
					CacheLocationId = _DEFAULT_CACHE_LOCATION_ID
				}
			};
		}
	}
}
