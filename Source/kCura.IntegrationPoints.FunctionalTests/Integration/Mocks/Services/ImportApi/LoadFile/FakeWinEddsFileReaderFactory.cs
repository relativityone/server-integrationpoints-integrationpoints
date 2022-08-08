using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile
{
    public class FakeWinEddsFileReaderFactory : IWinEddsFileReaderFactory
    {
        private readonly int _numberOfRecords;

        public FakeWinEddsFileReaderFactory(int numberOfRecords)
        {
            _numberOfRecords = numberOfRecords;
        }

        public IArtifactReader GetLoadFileReader(kCura.WinEDDS.LoadFile config)
        {
            return new FakeLoadFileArtifactReader(_numberOfRecords);
        }

        public IImageReader GetOpticonFileReader(ImageLoadFile config)
        {
            throw new System.NotImplementedException();
        }
    }
}