using System.Data;
using System.Text;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Authentication.Interfaces;

using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

using RAPI = Relativity;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        IFieldParserFactory _fieldParserFactory;
        IAuthenticatedCredentialProvider _credentialProvider;
        public DataReaderFactory(IAuthenticatedCredentialProvider credentialProvider, IFieldParserFactory fieldParserFactory)
        {
            _credentialProvider = credentialProvider;
            _fieldParserFactory = fieldParserFactory;
        }

        public IDataReader GetDataReader(string options)
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
            loadFile.SourceFileEncoding = Encoding.GetEncoding(settings.EncodingType);

            loadFile.LoadNativeFiles = false;
            loadFile.CreateFolderStructure = false;

            //Add columns to the LoadFile object
            IFieldParser fieldParser = _fieldParserFactory.GetFieldParser(options);
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
