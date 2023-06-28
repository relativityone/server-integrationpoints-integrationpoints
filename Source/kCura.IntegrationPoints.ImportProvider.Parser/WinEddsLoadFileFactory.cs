using System.Text;
using System.IO;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class WinEddsLoadFileFactory : IWinEddsLoadFileFactory
    {
        private readonly IDataTransferLocationServiceFactory _locationServiceFactory;
        private readonly IWebApiConfig _webApiConfig;
        private readonly IWinEddsBasicLoadFileFactory _basicLoadFileFactory;

        public WinEddsLoadFileFactory(IDataTransferLocationServiceFactory locationServiceFactory, IWebApiConfig webApiConfig, IWinEddsBasicLoadFileFactory basicLoadFileFactory)
        {
            _locationServiceFactory = locationServiceFactory;
            _webApiConfig = webApiConfig;
            _basicLoadFileFactory = basicLoadFileFactory;
        }

        public LoadFile GetLoadFile(ImportSettingsBase settings)
        {
            WinEDDS.Config.WebServiceURL = _webApiConfig.GetWebApiUrl;

            IDataTransferLocationService locationService = _locationServiceFactory.CreateService(settings.WorkspaceId);
            LoadFile loadFile = _basicLoadFileFactory.GetLoadFile(settings.WorkspaceId);

            loadFile.RecordDelimiter = (char)settings.AsciiColumn;
            loadFile.QuoteDelimiter = (char)settings.AsciiQuote;
            loadFile.NewlineDelimiter = (char)settings.AsciiNewLine;
            loadFile.MultiRecordDelimiter = (char)settings.AsciiMultiLine;
            loadFile.HierarchicalValueDelimiter = (char)settings.AsciiMultiLine;
            loadFile.FilePath = Path.Combine(locationService.GetWorkspaceFileLocationRootPath(settings.WorkspaceId), settings.LoadFile);
            loadFile.SourceFileEncoding = Encoding.GetEncoding(settings.EncodingType);
            loadFile.StartLineNumber = long.Parse(settings.LineNumber);

            loadFile.FirstLineContainsHeaders = true;

            loadFile.LoadNativeFiles = false;
            loadFile.CreateFolderStructure = false;

            return loadFile;
        }

        public ImageLoadFile GetImageLoadFile(ImportSettingsBase settings)
        {
            IDataTransferLocationService locationService = _locationServiceFactory.CreateService(settings.WorkspaceId);

            string loadFilePath = Path.Combine(locationService.GetWorkspaceFileLocationRootPath(settings.WorkspaceId), settings.LoadFile);
            return new ImageLoadFile {
                FileName = loadFilePath,
                StartLineNumber = long.Parse(settings.LineNumber)
            };
        }
    }
}
