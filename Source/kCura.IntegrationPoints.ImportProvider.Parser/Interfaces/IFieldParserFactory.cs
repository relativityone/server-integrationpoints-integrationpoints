using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IFieldParserFactory
    {
        IFieldParser GetFieldParser(ImportProviderSettings settings);
    }
}
