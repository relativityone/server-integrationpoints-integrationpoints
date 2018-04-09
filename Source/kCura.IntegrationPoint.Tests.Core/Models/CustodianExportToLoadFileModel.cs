using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class CustodianExportToLoadFileModel : IntegrationPointGeneralModel
    {
        public CustodianExportToLoadFileDetails ExportDetails { get; set; }

        public ExportToLoadFileOutputSettingsModel OutputSettings { get; set; }
        public CustodianExportToLoadFileModel(string name) : base(name)
        {
            DestinationProvider = INTEGRATION_POINT_PROVIDER_LOADFILE;
        }
    }
}
