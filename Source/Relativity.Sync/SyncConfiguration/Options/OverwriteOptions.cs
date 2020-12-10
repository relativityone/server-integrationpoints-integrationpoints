using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class OverwriteOptions
	{
		public ImportOverwriteMode OverwriteMode { get; set; }
		public FieldOverlayBehavior FieldsOverlayBehavior { get; set; }

		private OverwriteOptions()
		{ }

		public static OverwriteOptions AppendOnly()
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				FieldsOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};
		}

		public static OverwriteOptions AppendOverlay(FieldOverlayBehavior fieldOverlay = FieldOverlayBehavior.UseFieldSettings)
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.AppendOverlay,
				FieldsOverlayBehavior = fieldOverlay
			};
		}

		public static OverwriteOptions OverlayOnly(FieldOverlayBehavior fieldOverlay = FieldOverlayBehavior.UseFieldSettings)
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.OverlayOnly,
				FieldsOverlayBehavior = fieldOverlay
			};
		}
	}
}
