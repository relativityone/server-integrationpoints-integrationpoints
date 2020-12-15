using Relativity.Sync.Configuration;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class OverwriteOptions
	{
		public ImportOverwriteMode OverwriteMode { get; }

		public FieldOverlayBehavior FieldsOverlayBehavior { get; set; }

		public OverwriteOptions(ImportOverwriteMode overwriteMode)
		{
			OverwriteMode = overwriteMode;
		}
	}
}
