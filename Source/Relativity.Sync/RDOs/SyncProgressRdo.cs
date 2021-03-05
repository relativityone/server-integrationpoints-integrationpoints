using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
	[Rdo(SyncProgressGuids.ProgressObjectTypeGuid, "Sync Progress", SyncRdoGuids.SyncConfigurationGuid)]
	internal sealed class SyncProgressRdo : IRdoType
	{
		public int ArtifactId { get; set; }
		
		[RdoField(SyncProgressGuids.ExceptionGuid, RdoFieldType.LongText)]
		public string Exception { get; set; }

		[RdoField(SyncProgressGuids.MessageGuid, RdoFieldType.LongText)]
		public string Message { get; set; }

		[RdoField(SyncProgressGuids.OrderGuid, RdoFieldType.WholeNumber)]
		public int Order { get; set; }

		[RdoField(SyncProgressGuids.StatusGuid, RdoFieldType.FixedLengthText)]
		public string Status { get; set; }
	}
}