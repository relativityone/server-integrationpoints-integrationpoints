﻿using System.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Models;

using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

using RAPI = Relativity;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        ICredentialProvider _credentialProvider;
        IFieldParserFactory _fieldParserFactory;
        //public FieldParserFactory(ICredentialProvider credentialProvider, IWebApiConfig webApiConfig)
        public DataReaderFactory(ICredentialProvider credentialProvider, IFieldParserFactory fieldParserFactory)
        {
            _credentialProvider = credentialProvider;
            _fieldParserFactory = fieldParserFactory;
        }

        public IDataReader GetDataReader(string options)
        {
            //Need to add columns to the LoadFile object
            IFieldParser fieldParser = _fieldParserFactory.GetFieldParser(options);

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
            var colIdx = 0;
            foreach (var col in fieldParser.GetFields())
            {
                var fieldCat = -1;
                //TODO: instead of setting the first column as the identifier, use options
                if (colIdx == 0)
                {
                    fieldCat = (int)RAPI.FieldCategory.Identifier;
                }

                var newDocField = new kCura.WinEDDS.DocumentField(col, colIdx, 4, fieldCat, -1, -1, -1, false,
                    kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                var mapItem = new kCura.WinEDDS.LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);
                loadFile.FieldMap.Add(mapItem);
                colIdx++;
            }


            return new LoadFileDataReader(loadFile);
        }
    }
}
