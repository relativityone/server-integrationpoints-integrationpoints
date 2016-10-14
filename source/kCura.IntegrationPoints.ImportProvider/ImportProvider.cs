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

        public ImportProvider(IFieldParserFactory fieldParserFactory, IDataReaderFactory dataReaderFactory)
        {
            _fieldParserFactory = fieldParserFactory;
            _dataReaderFactory = dataReaderFactory;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            return _dataReaderFactory.GetDataReader(options);
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines, string options)
        {
            var Dt = new DataTable();
            foreach (var field in fields)
            {
                Dt.Columns.Add(field.FieldIdentifier);
            }

            foreach (var line in sourceFileLines)
            {
                var lineSplit = line.Split(',');
                var dtRow = Dt.NewRow();
                foreach (var field in fields)
                {
                    dtRow[field.FieldIdentifier] = lineSplit[Int32.Parse(field.FieldIdentifier)];
                }
                Dt.Rows.Add(dtRow);
            }

            return Dt.CreateDataReader();
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
