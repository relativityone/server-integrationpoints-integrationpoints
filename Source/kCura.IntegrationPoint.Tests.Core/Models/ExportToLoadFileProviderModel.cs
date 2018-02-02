using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class ExportToLoadFileProviderModel : IntegrationPointGeneralModel
	{
		public ExportToLoadFileProviderModel(string ripName) : base(ripName)
		{
			Scheduler = null;

			DestinationProvider = INTEGRATION_POINT_PROVIDER_LOADFILE;

			ExportImages = false;
			ExportNatives = false;
			TextFieldsAsFiles = false;
			DestinationFolder = string.Empty;
			CreateExportFolder = true;
			OverwriteFiles = false;
		}
		
		public SchedulerModel Scheduler { get; set; }

		public List<string> SelectedFields { get; set; }

		#region "Source Detail"

		public string Source { get; set; }
		public string SavedSearch { get; set; }
		public int StartAtRecord { get; set; }

		#endregion

		#region "Export Detail"

		public bool? ExportImages { get; set; }
		public bool? ExportNatives { get; set; }
		public bool? TextFieldsAsFiles { get; set; }
		public string DestinationFolder { get; set; }
		public bool? CreateExportFolder { get; set; }
		public bool? OverwriteFiles { get; set; }

		#endregion
	}
}
