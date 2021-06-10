using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile
{
	public class FakeWinEddsFileReaderFactory : IWinEddsFileReaderFactory
	{
		public IArtifactReader GetLoadFileReader(kCura.WinEDDS.LoadFile config)
		{
			return new FakeLoadFileArtifactReader();
		}

		public IImageReader GetOpticonFileReader(ImageLoadFile config)
		{
			throw new System.NotImplementedException();
		}
	}
}