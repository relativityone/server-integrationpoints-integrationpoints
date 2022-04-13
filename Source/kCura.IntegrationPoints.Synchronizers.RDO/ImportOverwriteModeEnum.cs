using System.ComponentModel;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public enum ImportOverwriteModeEnum
	{
		[Description("Append Only")]
		AppendOnly = Relativity.DataReaderClient.OverwriteModeEnum.Append,

		[Description("Append/Overlay")]
		AppendOverlay = Relativity.DataReaderClient.OverwriteModeEnum.AppendOverlay,

		[Description("Overlay Only")]
		OverlayOnly = Relativity.DataReaderClient.OverwriteModeEnum.Overlay,
	}
}
