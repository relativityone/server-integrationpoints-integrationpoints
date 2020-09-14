namespace Relativity.Sync.Transfer
{
	internal class ImageFile
	{
		public ImageFile(int documentArtifactId, string location, string filename, long size, int? productionId = null)
		{
			DocumentArtifactId = documentArtifactId;
			Location = location;
			Filename = filename;
			Size = size;
			ProductionId = productionId;
		}

		public int DocumentArtifactId { get; }
		public string Location { get; }
		public string Filename { get; }
		public long Size { get; }
		public int? ProductionId { get; }
	}
}