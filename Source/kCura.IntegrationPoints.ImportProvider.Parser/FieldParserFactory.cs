using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FieldParserFactory : IFieldParserFactory
    {
        private readonly IWinEddsLoadFileFactory _winEddsLoadFileFactory;
        private readonly IWinEddsFileReaderFactory _winEddsFileReaderFactory;

        public FieldParserFactory(IWinEddsLoadFileFactory winEddsLoadFileFactory, IWinEddsFileReaderFactory winEddsFileReaderFactory)
        {
            _winEddsLoadFileFactory = winEddsLoadFileFactory;
            _winEddsFileReaderFactory = winEddsFileReaderFactory;
        }

        public IFieldParser GetFieldParser(ImportProviderSettings settings)
        {
            LoadFile config = _winEddsLoadFileFactory.GetLoadFile(settings);
            kCura.WinEDDS.Api.IArtifactReader loadFileReader = _winEddsFileReaderFactory.GetLoadFileReader(config);
            return new LoadFileFieldParser(config, loadFileReader);
        }
    }
}
