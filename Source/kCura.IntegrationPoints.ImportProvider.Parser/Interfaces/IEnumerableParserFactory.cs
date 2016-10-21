using System.Collections.Generic;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IEnumerableParserFactory
    {
        IEnumerable<string[]> GetEnumerableParser(IEnumerable<string> sourceFileLines, string options);
    }
}
