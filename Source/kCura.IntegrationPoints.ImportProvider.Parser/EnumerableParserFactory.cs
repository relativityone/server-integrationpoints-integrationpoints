using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class EnumerableParserFactory : IEnumerableParserFactory
	{
		public IEnumerable<string[]> GetEnumerableParser(IEnumerable<string> sourceFileLines, ImportProviderSettings settings)
		{
			if (int.Parse(settings.ImportType) == (int)ImportType.ImportTypeValue.Document)
			{
				return new EnumerableParser(sourceFileLines, (char)settings.AsciiColumn, (char)settings.AsciiQuote);
			}
			else
			{
				return new EnumerableParser(sourceFileLines, ',', '"');
			}
		}
	}
}
