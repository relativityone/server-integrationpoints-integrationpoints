using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public static OverwriteOptions AppendOverlay(FieldOverlayBehavior fieldOverlay)
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.AppendOverlay,
				FieldsOverlayBehavior = fieldOverlay
			};
		}

		public static OverwriteOptions OverlayOnly(FieldOverlayBehavior fieldOverlay)
		{
			return new OverwriteOptions
			{
				OverwriteMode = ImportOverwriteMode.OverlayOnly,
				FieldsOverlayBehavior = fieldOverlay
			};
		}
	}
}
