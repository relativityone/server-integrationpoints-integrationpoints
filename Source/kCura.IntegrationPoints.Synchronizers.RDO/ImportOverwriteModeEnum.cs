namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public enum ImportOverwriteModeEnum
	{
		AppendOnly = kCura.Relativity.DataReaderClient.OverwriteModeEnum.Append,
		AppendOverlay = kCura.Relativity.DataReaderClient.OverwriteModeEnum.AppendOverlay,
		OverlayOnly = kCura.Relativity.DataReaderClient.OverwriteModeEnum.Overlay,
	}
}
