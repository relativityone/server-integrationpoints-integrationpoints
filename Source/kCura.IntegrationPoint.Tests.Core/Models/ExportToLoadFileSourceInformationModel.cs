using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ExportToLoadFileSourceInformationModel
    {
        public ExportToLoadFileSourceInformationModel(string savedSearch)
        {
            this.InitializePropertyDefaultValues();
            SavedSearch = savedSearch;
            SelectedFields = new List<string>();
        }

        [DefaultValue("Saved Search")]
        public string Source { get; set; }

        public string SavedSearch { get; set; }

        public string ProductionSet { get; set; }

        public string Folder { get; set; }

        public string View { get; set; }

        [DefaultValue(1)]
        public int StartAtRecord { get; set; }

        [DefaultValue(true)]
        public bool SelectAllFields { get; set; }

        public List<string> SelectedFields { get; set; }
    }
}