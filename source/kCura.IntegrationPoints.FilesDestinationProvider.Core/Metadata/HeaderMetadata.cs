namespace kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata
{
	public struct HeaderMetadata
	{
		public string DisplayName { get; private set; }

		public HeaderMetadata(string displayName)
		{
			DisplayName = displayName;
		}
	}
}
