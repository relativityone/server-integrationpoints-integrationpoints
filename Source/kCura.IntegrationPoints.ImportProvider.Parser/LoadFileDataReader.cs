using System.Data;
using System.Collections.Generic;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFileDataReader : LoadFileBase
    {
        public LoadFileDataReader(kCura.WinEDDS.LoadFile config)
            : base(config)
        {
        }
    }
}
