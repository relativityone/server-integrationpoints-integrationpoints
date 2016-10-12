using Newtonsoft.Json;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Authentication.Interfaces;

using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FieldParserFactory : IFieldParserFactory
    {
        //TODO: Figure out why IWebApiConfig cannot be injected
        //IWebApiConfig _webApiConfig;
        IAuthenticatedCredentialProvider _credentialProvider;
        //public FieldParserFactory(ICredentialProvider credentialProvider, IWebApiConfig webApiConfig)
        public FieldParserFactory(IAuthenticatedCredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public IFieldParser GetFieldParser(string options)
        {
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);

            var factory = new kCura.WinEDDS.NativeSettingsFactory(_credentialProvider.GetAuthenticatedCredential(), settings.WorkspaceId);
            var loadFile = factory.ToLoadFile();

            loadFile.RecordDelimiter = (char)settings.AsciiColumn;
            loadFile.QuoteDelimiter = (char)settings.AsciiQuote;
            loadFile.NewlineDelimiter = (char)settings.AsciiNewLine;
            loadFile.MultiRecordDelimiter = (char)settings.AsciiMultiLine;
            loadFile.HierarchicalValueDelimiter = (char)settings.AsciiMultiLine;
            loadFile.FilePath = settings.LoadFile;

            loadFile.LoadNativeFiles = false;
            loadFile.CreateFolderStructure = false;

            return new LoadFileFieldParser(loadFile);
        }
    }
}
