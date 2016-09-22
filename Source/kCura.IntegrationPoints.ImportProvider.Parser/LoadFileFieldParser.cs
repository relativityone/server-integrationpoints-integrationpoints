using System.Collections.Generic;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFileFieldParser : IFieldParser
    {
        private kCura.WinEDDS.LoadFileReader _loadFileReader;
        private kCura.WinEDDS.LoadFile _config;

        public LoadFileFieldParser(kCura.WinEDDS.LoadFile loadFile)
        {
            _config = loadFile;
            _loadFileReader = new kCura.WinEDDS.LoadFileReader(_config, false);
        }

        public List<string> GetFields()
        {
            return new List<string>(_loadFileReader.GetColumnNames(_config));
        }
    }
}
