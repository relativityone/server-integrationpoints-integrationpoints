using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ViewResultsModel
    {
        public List<ViewModel> Results { get; set; }

        public int TotalResults { get; set; }

        public bool HasMoreResults { get; set; }
    }
}
