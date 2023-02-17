using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile
{
    public class FakeWinEddsBasicLoadFileFactory : IWinEddsBasicLoadFileFactory
    {
        public kCura.WinEDDS.LoadFile GetLoadFile(int workspaceId)
        {
            return new kCura.WinEDDS.LoadFile();
        }
    }
}
