using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFileBase
    {
        protected kCura.WinEDDS.LoadFileReader _loadFileReader;
        protected kCura.WinEDDS.LoadFile _config;

        protected LoadFileBase(kCura.WinEDDS.LoadFile loadFile)
        {
            Init(loadFile);
        }

        private void Init(kCura.WinEDDS.LoadFile loadFile)
        {
            _config = loadFile;
            _loadFileReader = new kCura.WinEDDS.LoadFileReader(_config, false);
        }
    }
}
