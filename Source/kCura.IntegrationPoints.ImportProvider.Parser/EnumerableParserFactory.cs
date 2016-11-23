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
        public IEnumerable<string[]> GetEnumerableParser(IEnumerable<string> sourceFileLines, string options)
        {
            ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
            return new EnumerableParser(sourceFileLines, (char)settings.AsciiColumn, (char)settings.AsciiQuote);
        }
    }
}
