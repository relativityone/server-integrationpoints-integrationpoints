namespace Relativity.Sync.Transfer
{
	internal sealed class ItemStatus
	{
		public int ArtifactId { get; }
		public bool? IsSuccessful { get; set; }

		public ItemStatus(int artifactId)
		{
			ArtifactId = artifactId;
		}
	}
}