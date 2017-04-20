using System.Collections.Generic;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class LoadFileFieldParser : IFieldParser
	{
		private kCura.WinEDDS.LoadFile _config;
		private kCura.WinEDDS.Api.IArtifactReader _loadFileReader;

		public LoadFileFieldParser(kCura.WinEDDS.LoadFile config, kCura.WinEDDS.Api.IArtifactReader loadFileReader)
		{
			_config = config;
			_loadFileReader = loadFileReader;
		}

		public List<string> GetFields()
		{
			return new List<string>(_loadFileReader.GetColumnNames(_config));
		}
	}
}
