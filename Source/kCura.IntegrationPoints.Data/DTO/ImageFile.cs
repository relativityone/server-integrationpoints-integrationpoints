namespace kCura.IntegrationPoints.Data.DTO
{
	public class ImageFile
	{
		public ImageFile(int documentArtifactId, string location, string filename, long size, int? productionId = null,
			string nativeIdentifier = null)
		{
			DocumentArtifactId = documentArtifactId;
			Location = location;
			Filename = filename;
			Size = size;
			ProductionId = productionId;
			NativeIdentifier = nativeIdentifier;
		}

		public int DocumentArtifactId { get; }
		public string Location { get; }
		public string Filename { get; }
		public long Size { get; }
		public int? ProductionId { get; }
		public string NativeIdentifier { get; }
	}
}
