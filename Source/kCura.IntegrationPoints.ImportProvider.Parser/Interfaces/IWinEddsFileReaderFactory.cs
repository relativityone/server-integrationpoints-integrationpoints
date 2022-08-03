using kCura.WinEDDS;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IWinEddsFileReaderFactory
    {
        IArtifactReader GetLoadFileReader(LoadFile config);
        IImageReader GetOpticonFileReader(ImageLoadFile config);
    }
}
