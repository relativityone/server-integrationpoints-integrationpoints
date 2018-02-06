namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.Collections.Generic;
	using System.ComponentModel;

	public class ExportToLoadFileSourceInformationModel
	{
		public ExportToLoadFileSourceInformationModel(string savedSearch)
		{
			SavedSearch = savedSearch;
		}

		[DefaultValue("Saved Search")]
		public string Source { get; set; }

		public string SavedSearch { get; set; }

		[DefaultValue(1)]
		public int StartAtRecord { get; set; }

		public List<string> SelectedFields { get; set; }
	}
}