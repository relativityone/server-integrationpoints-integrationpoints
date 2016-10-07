using Newtonsoft.Json;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Models;

using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FieldParserFactory : IFieldParserFactory
    {
        //TODO: Figure out why IWebApiConfig cannot be injected
        //IWebApiConfig _webApiConfig;
        ICredentialProvider _credentialProvider;
        //public FieldParserFactory(ICredentialProvider credentialProvider, IWebApiConfig webApiConfig)
        public FieldParserFactory(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public IFieldParser GetFieldParser(string options)
        {
            //Extract file path from settings object
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
            var filePath = settings.LoadFile;

            //Set up Config object with WebAPI link
            var webApiConfig = new WebApiConfig();
            WinEDDS.Config.WebServiceURL = webApiConfig.GetWebApiUrl;

            var cookieContainer = new System.Net.CookieContainer();
            var credential = _credentialProvider.Authenticate(cookieContainer);

            //TODO: replace hard coded workspace with value from helper
            var factory = new kCura.WinEDDS.NativeSettingsFactory(credential, 1016969);
            var loadFile = factory.ToLoadFile();
            loadFile.RecordDelimiter = ',';
            loadFile.FilePath = filePath;
            loadFile.LoadNativeFiles = false;
            loadFile.CreateFolderStructure = false;

            return new LoadFileFieldParser(loadFile);
        }
    }
}
