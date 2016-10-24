using System.Data;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

using RAPI = Relativity;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        IWinEddsLoadFileFactory _winEddsLoadFileFactory;
        IFieldParserFactory _fieldParserFactory;
        public DataReaderFactory(IFieldParserFactory fieldParserFactory, IWinEddsLoadFileFactory winEddsLoadFileFactory)
        {
            _fieldParserFactory = fieldParserFactory;
            _winEddsLoadFileFactory = winEddsLoadFileFactory;
        }

        public IDataReader GetDataReader(string options)
        {
            ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
            LoadFile loadFile = _winEddsLoadFileFactory.GetLoadFile(settings);

            //Add columns to the LoadFile object
            IFieldParser fieldParser = _fieldParserFactory.GetFieldParser(options);
            int colIdx = 0;
            foreach (string col in fieldParser.GetFields())
            {
                int fieldCat = -1;
                //TODO: instead of setting the first column as the identifier, use options
                if (colIdx == 0)
                {
                    fieldCat = (int)RAPI.FieldCategory.Identifier;
                }

                DocumentField newDocField = new DocumentField(col, colIdx, 4, fieldCat, -1, -1, -1, false,
                    kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                LoadFileFieldMap.LoadFileFieldMapItem mapItem = new LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);
                loadFile.FieldMap.Add(mapItem);
                colIdx++;
            }

            LoadFileDataReader rv = new LoadFileDataReader(loadFile);
            rv.Init(); //Init performs unsafe operations, so they could not be performed in the constructor.
            return rv;
        }
    }
}
