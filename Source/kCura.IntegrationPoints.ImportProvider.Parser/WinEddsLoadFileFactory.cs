using System.Text;
using System.Net;
using System.IO;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class WinEddsLoadFileFactory : IWinEddsLoadFileFactory
	{
		private ICredentialProvider _credentialProvider;
		private IDataTransferLocationServiceFactory _locationServiceFactory;
		private IDataTransferLocationService _locationService;
		private IWebApiConfig _webApiConfig;

		public WinEddsLoadFileFactory(ICredentialProvider credentialProvider, IDataTransferLocationServiceFactory locationServiceFactory, IWebApiConfig webApiConfig)
		{
			_credentialProvider = credentialProvider;
			_locationServiceFactory = locationServiceFactory;
			_webApiConfig = webApiConfig;
		}

		public LoadFile GetLoadFile(ImportSettingsBase settings)
		{
			WinEDDS.Config.WebServiceURL = _webApiConfig.GetWebApiUrl;
			NetworkCredential cred = _credentialProvider.Authenticate(new System.Net.CookieContainer());

			NativeSettingsFactory factory = new NativeSettingsFactory(cred, settings.WorkspaceId);
			LoadFile loadFile = factory.ToLoadFile();

			_locationService = _locationServiceFactory.CreateService(settings.WorkspaceId);

			loadFile.RecordDelimiter = (char)settings.AsciiColumn;
			loadFile.QuoteDelimiter = (char)settings.AsciiQuote;
			loadFile.NewlineDelimiter = (char)settings.AsciiNewLine;
			loadFile.MultiRecordDelimiter = (char)settings.AsciiMultiLine;
			loadFile.HierarchicalValueDelimiter = (char)settings.AsciiMultiLine;
			loadFile.FilePath = Path.Combine(_locationService.GetWorkspaceFileLocationRootPath(settings.WorkspaceId), settings.LoadFile);
			loadFile.SourceFileEncoding = Encoding.GetEncoding(settings.EncodingType);
			loadFile.StartLineNumber = long.Parse(settings.LineNumber);

			loadFile.FirstLineContainsHeaders = true;

			loadFile.LoadNativeFiles = false;
			loadFile.CreateFolderStructure = false;

			return loadFile;
		}

		public ImageLoadFile GetImageLoadFile(ImportSettingsBase settings)
		{
			_locationService = _locationServiceFactory.CreateService(settings.WorkspaceId);

			string loadFilePath = Path.Combine(_locationService.GetWorkspaceFileLocationRootPath(settings.WorkspaceId), settings.LoadFile);
			return new ImageLoadFile {
				FileName = loadFilePath,
				StartLineNumber = long.Parse(settings.LineNumber)
			};
		}
	}
}
