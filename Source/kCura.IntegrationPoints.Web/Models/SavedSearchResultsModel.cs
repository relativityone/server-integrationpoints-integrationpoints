using System.Collections.Generic;

namespace kCura.IntegrationPoints.Web.Models
{
    public class SavedSearchResultsModel
    {
        public List<SavedSearchModel> Results { get; set; }

        public int TotalResults { get; set; }

        public bool HasMoreResults { get; set; }
    }
}
