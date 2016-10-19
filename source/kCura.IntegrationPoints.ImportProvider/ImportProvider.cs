using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider
{
    [kCura.IntegrationPoints.Contracts.DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
    public class ImportProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
    {
        private IFieldParserFactory _fieldParserFactory;
        private IDataReaderFactory _dataReaderFactory;
        private IEnumerableParserFactory _enumerableParserFactory;

        public ImportProvider(IFieldParserFactory fieldParserFactory, IDataReaderFactory dataReaderFactory, IEnumerableParserFactory enumerableParserFactory)
        {
            _fieldParserFactory = fieldParserFactory;
            _dataReaderFactory = dataReaderFactory;
            _enumerableParserFactory = enumerableParserFactory;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            return _dataReaderFactory.GetDataReader(options);
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines, string options)
        {
            IEnumerable<string[]> enumerableParser = _enumerableParserFactory.GetEnumerableParser(sourceFileLines, options);

            DataTable dt = new DataTable();
            foreach (FieldEntry field in fields)
            {
                dt.Columns.Add(field.FieldIdentifier);
            }

            foreach (string[] sourceRow in enumerableParser)
            {
                DataRow dtRow = dt.NewRow();
                foreach (FieldEntry field in fields)
                {
                    dtRow[field.FieldIdentifier] = sourceRow[Int32.Parse(field.FieldIdentifier)];
                }
                dt.Rows.Add(dtRow);
            }

            return dt.CreateDataReader();
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
                    FieldIdentifier = idx.ToString(),
                    FieldType = FieldType.String,
                    IsIdentifier = idx == 0
                });
                idx++;
            }
            return result;
        }
    }
}
