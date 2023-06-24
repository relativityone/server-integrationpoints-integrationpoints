using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        private readonly IWinEddsLoadFileFactory _winEddsLoadFileFactory;
        private readonly IWinEddsFileReaderFactory _winEddsFileReaderFactory;
        private readonly IFieldParserFactory _fieldParserFactory;
        private readonly ISerializer _serializer;
        private readonly IReadOnlyFileMetadataStore _readOnlyFileMetadataStore;
        private readonly IDiagnosticLog _diagnosticLogger;
        private readonly IAPILog _logger;

        public DataReaderFactory(
            IFieldParserFactory fieldParserFactory,
            IWinEddsLoadFileFactory winEddsLoadFileFactory,
            IWinEddsFileReaderFactory winEddsFileReaderFactory,
            ISerializer serializer,
            IReadOnlyFileMetadataStore readOnlyFileMetadataStore,
            IDiagnosticLog diagnosticLogger,
            IAPILog logger)
        {
            _fieldParserFactory = fieldParserFactory;
            _winEddsLoadFileFactory = winEddsLoadFileFactory;
            _winEddsFileReaderFactory = winEddsFileReaderFactory;
            _serializer = serializer;
            _readOnlyFileMetadataStore = readOnlyFileMetadataStore;
            _diagnosticLogger = diagnosticLogger;
            _logger = logger;
        }

        public IDataReader GetDataReader(FieldMap[] fieldMaps, string options, IJobStopManager jobStopManager, bool addExtraNativeColumns)
        {
            ImportProviderSettings providerSettings = _serializer.Deserialize<ImportProviderSettings>(options);
            if (int.Parse(providerSettings.ImportType) == (int)ImportType.ImportTypeValue.Document)
            {
                LoadFileDataReader lfdr = GetLoadFileDataReader(fieldMaps, providerSettings, jobStopManager, addExtraNativeColumns);
                ImportDataReader idr = new ImportDataReader(lfdr);
                idr.Setup(fieldMaps);
                return idr;
            }
            else
            {
                return GetOpticonDataReader(providerSettings, jobStopManager);
            }
        }

        public INativeFilePathReader GetNativeFilePathReader(FieldMap[] fieldMaps, string options, IJobStopManager jobStopManager)
        {
            ImportProviderSettings providerSettings = _serializer.Deserialize<ImportProviderSettings>(options);
            LoadFileDataReader lfdr = GetLoadFileDataReader(fieldMaps, providerSettings, jobStopManager, addExtraNativeColumns: false);
            ImportDataReader idr = new ImportDataReader(lfdr);
            idr.Setup(fieldMaps);
            return idr;
        }

        private OpticonDataReader GetOpticonDataReader(ImportProviderSettings settings, IJobStopManager jobStopManager)
        {
            ImageLoadFile config = _winEddsLoadFileFactory.GetImageLoadFile(settings);
            IImageReader reader = _winEddsFileReaderFactory.GetOpticonFileReader(config);
            OpticonDataReader rv = new OpticonDataReader(settings, reader, jobStopManager);
            rv.Init();
            return rv;
        }

        private LoadFileDataReader GetLoadFileDataReader(FieldMap[] fieldMaps, ImportProviderSettings settings, IJobStopManager jobStopManager, bool addExtraNativeColumns)
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

            _logger.LogInformation("ImportProviderSettings: {@settings}", settings);

            LoadFileDataReader rv = new LoadFileDataReader(settings, loadFile, reader, jobStopManager, _readOnlyFileMetadataStore, _diagnosticLogger, addExtraNativeColumns);
            rv.Init();
            return rv;
        }
    }
}
