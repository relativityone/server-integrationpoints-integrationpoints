using System.Text;
using System.Net;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class WinEddsLoadFileFactory : IWinEddsLoadFileFactory
    {
        ICredentialProvider _credentialProvider;
        public WinEddsLoadFileFactory(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public LoadFile GetLoadFile(ImportSettingsBase settings)
        {
            WinEDDS.Config.WebServiceURL = (new WebApiConfig()).GetWebApiUrl;
            NetworkCredential cred = _credentialProvider.Authenticate(new System.Net.CookieContainer());

            NativeSettingsFactory factory = new NativeSettingsFactory(cred, settings.WorkspaceId);
            LoadFile loadFile = factory.ToLoadFile();

            loadFile.RecordDelimiter = (char)settings.AsciiColumn;
            loadFile.QuoteDelimiter = (char)settings.AsciiQuote;
            loadFile.NewlineDelimiter = (char)settings.AsciiNewLine;
            loadFile.MultiRecordDelimiter = (char)settings.AsciiMultiLine;
            loadFile.HierarchicalValueDelimiter = (char)settings.AsciiMultiLine;
            loadFile.FilePath = settings.LoadFile;
            loadFile.SourceFileEncoding = Encoding.GetEncoding(settings.EncodingType);
            loadFile.StartLineNumber = long.Parse(settings.LineNumber);

            loadFile.FirstLineContainsHeaders = true;

            loadFile.LoadNativeFiles = false;
            loadFile.CreateFolderStructure = false;

            return loadFile;
        }
    }
}
