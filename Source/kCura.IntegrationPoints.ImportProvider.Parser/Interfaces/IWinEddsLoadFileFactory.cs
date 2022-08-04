using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IWinEddsLoadFileFactory
    {
        LoadFile GetLoadFile(ImportSettingsBase settings);
        ImageLoadFile GetImageLoadFile(ImportSettingsBase settings);
    }
}
