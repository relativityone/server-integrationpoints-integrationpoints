using System;
using System.Collections.Generic;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.ImportProvider
{
    [DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
    public class ImportProvider : IDataSourceProvider
    {
        private readonly IFieldParserFactory _fieldParserFactory;
        private readonly ISerializer _serializer;

        public ImportProvider(IFieldParserFactory fieldParserFactory,
            ISerializer serializer)
        {
            _fieldParserFactory = fieldParserFactory;
            _serializer = serializer;
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(providerConfiguration.Configuration);
            IFieldParser parser = _fieldParserFactory.GetFieldParser(settings);
            List<FieldEntry> result = new List<FieldEntry>();
            int idx = 0;
            foreach (string fieldName in parser.GetFields())
            {
                result.Add(new FieldEntry
                {
                    DisplayName = fieldName,
                    FieldIdentifier = idx.ToString(),
                    FieldType = FieldType.String
                });
                idx++;
            }
            return result;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
        {
            throw new NotImplementedException("GetBatchableIds should not be called on ImportProvider");
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines, DataSourceProviderConfiguration providerConfiguration)
        {
            throw new NotImplementedException("GetData should not be called on ImportProvider");
        }
    }
}
