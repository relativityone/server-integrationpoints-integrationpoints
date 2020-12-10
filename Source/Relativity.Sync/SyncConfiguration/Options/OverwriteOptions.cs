using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class OverwriteOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public ImportOverwriteMode OverwriteMode { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public FieldOverlayBehavior FieldsOverlayBehavior { get; set; }

		private OverwriteOptions()
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static OverwriteOptions AppendOnly()
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				FieldsOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fieldOverlay"></param>
		/// <returns></returns>
		public static OverwriteOptions AppendOverlay(FieldOverlayBehavior fieldOverlay = FieldOverlayBehavior.UseFieldSettings)
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.AppendOverlay,
				FieldsOverlayBehavior = fieldOverlay
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fieldOverlay"></param>
		/// <returns></returns>
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
