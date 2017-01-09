using System.Collections.Generic;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class LoadFileFieldParser : LoadFileBase, IFieldParser
	{
		public LoadFileFieldParser(kCura.WinEDDS.LoadFile config)
			: base(config)
		{
		}

		public List<string> GetFields()
		{
			return new List<string>(_loadFileReader.GetColumnNames(_config));
		}
	}
}
