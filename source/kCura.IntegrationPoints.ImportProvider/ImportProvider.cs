using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider
{
    [kCura.IntegrationPoints.Contracts.DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
    public class ImportProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
    {
        private IFieldParserFactory _fieldParserFactory;
        public ImportProvider(IFieldParserFactory fieldParserFactory)
        {
            _fieldParserFactory = fieldParserFactory;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            var parser = _fieldParserFactory.GetFieldParser(options);
            var result = new List<FieldEntry>();
            var idx = 0;
            foreach (var fieldName in parser.GetFields())
            {
                result.Add(new FieldEntry
                {
                    DisplayName = fieldName,
                    FieldIdentifier = fieldName,
                    FieldType = FieldType.String,
                    IsIdentifier = idx++ == 0
                });
            }
            return result;

            /*
            SeqLogger.Info("Got Options: {Options}", options);

            var result = new List<FieldEntry>();
            result.Add(new FieldEntry
            {
                DisplayName = "JustThisOneField",
                FieldIdentifier = "JustThisOneField",
                FieldType = FieldType.String,
                IsIdentifier = true
            });

            SeqLogger.Info("Finished constructing result. About to return...");

            return result;
            */
        }
    }
}
