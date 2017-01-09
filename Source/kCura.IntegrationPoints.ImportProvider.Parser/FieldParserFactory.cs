using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class FieldParserFactory : IFieldParserFactory
	{
		IWinEddsLoadFileFactory _winEddsLoadFileFactory;
		public FieldParserFactory(IWinEddsLoadFileFactory winEddsLoadFileFactory)
		{
			_winEddsLoadFileFactory = winEddsLoadFileFactory;
		}

		public IFieldParser GetFieldParser(ImportProviderSettings settings)
		{
			return new LoadFileFieldParser(_winEddsLoadFileFactory.GetLoadFile(settings));
		}
	}
}
