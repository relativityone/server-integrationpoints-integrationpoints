namespace Relativity.Sync.Configuration
{
	internal interface ITemporaryStorageInitializationConfiguration : IConfiguration
	{
		bool IsStorageIdSet { get; }

		int StorageId { get; set; }
	}
}