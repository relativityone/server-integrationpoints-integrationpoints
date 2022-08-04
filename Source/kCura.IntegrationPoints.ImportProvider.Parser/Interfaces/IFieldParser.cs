using System.Collections.Generic;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    /// <summary>
    /// IFieldParser classes can return a list fields from a delimited file
    /// </summary>
    public interface IFieldParser
    {
        List<string> GetFields();
    }
}
