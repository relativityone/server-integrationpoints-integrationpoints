using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IEnumerableParserFactory
    {
        IEnumerable<string[]> GetEnumerableParser(IEnumerable<string> sourceFileLines, ImportProviderSettings settings);
    }
}
