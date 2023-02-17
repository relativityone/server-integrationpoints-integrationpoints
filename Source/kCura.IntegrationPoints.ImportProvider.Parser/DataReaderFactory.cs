using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        private readonly IWinEddsLoadFileFactory _winEddsLoadFileFactory;
        private readonly IWinEddsFileReaderFactory _winEddsFileReaderFactory;
        private readonly IFieldParserFactory _fieldParserFactory;
        private readonly ISerializer _serializer;

        public DataReaderFactory(IFieldParserFactory fieldParserFactory,
            IWinEddsLoadFileFactory winEddsLoadFileFactory,
            IWinEddsFileReaderFactory winEddsFileReaderFactory,
            ISerializer serializer)
        {
            _fieldParserFactory = fieldParserFactory;
            _winEddsLoadFileFactory = winEddsLoadFileFactory;
            _winEddsFileReaderFactory = winEddsFileReaderFactory;
            _serializer = serializer;
        }

        public IDataReader GetDataReader(FieldMap[] fieldMaps, string options, IJobStopManager jobStopManager)
        {
            ImportProviderSettings providerSettings = _serializer.Deserialize<ImportProviderSettings>(options);
            if (int.Parse(providerSettings.ImportType) == (int)ImportType.ImportTypeValue.Document)
            {
                LoadFileDataReader lfdr = GetLoadFileDataReader(fieldMaps, providerSettings, jobStopManager);
                ImportDataReader idr = new ImportDataReader(lfdr);
                idr.Setup(fieldMaps);
                return idr;
            }
            else
            {
                return GetOpticonDataReader(providerSettings, jobStopManager);
            }
        }

        private OpticonDataReader GetOpticonDataReader(ImportProviderSettings settings, IJobStopManager jobStopManager)
        {
            ImageLoadFile config = _winEddsLoadFileFactory.GetImageLoadFile(settings);
            IImageReader reader = _winEddsFileReaderFactory.GetOpticonFileReader(config);
            OpticonDataReader rv = new OpticonDataReader(settings, reader, jobStopManager);
            rv.Init();
            return rv;
        }

        private LoadFileDataReader GetLoadFileDataReader(FieldMap[] fieldMaps, ImportProviderSettings settings, IJobStopManager jobStopManager)
        {
            string fieldIdentifierColumnName = fieldMaps.FirstOrDefault(x => x.SourceField.IsIdentifier)?.SourceField.DisplayName;

            LoadFile loadFile = _winEddsLoadFileFactory.GetLoadFile(settings);

            // Add columns to the LoadFile object
            IFieldParser fieldParser = _fieldParserFactory.GetFieldParser(settings);
            int colIdx = 0;
            foreach (string col in fieldParser.GetFields())
            {
                int fieldCat = (! string.IsNullOrEmpty(fieldIdentifierColumnName) && col == fieldIdentifierColumnName)
                    ? (int)FieldCategory.Identifier
                    : -1;

                DocumentField newDocField = new DocumentField(col, colIdx, 4, fieldCat, -1, -1, -1, false,
                    kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                LoadFileFieldMap.LoadFileFieldMapItem mapItem = new LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx++);
                loadFile.FieldMap.Add(mapItem);
            }

            IArtifactReader reader = _winEddsFileReaderFactory.GetLoadFileReader(loadFile);

            LoadFileDataReader rv = new LoadFileDataReader(settings, loadFile, reader, jobStopManager);
            rv.Init();
            return rv;
        }
    }
}
