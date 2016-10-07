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

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        {
            SeqLogger.Info("ImportProvider::GetData()");

            var ColumnIndices = new Dictionary<string, int>();
            var Dt = new DataTable();
            var requestedFieldList = new List<string>();
            foreach (var field in fields)
            {
                SeqLogger.Info("Field: {DisplayName}, {FieldIdentifier}, {IsIdentifier}", field.DisplayName, field.FieldIdentifier, field.IsIdentifier);
                requestedFieldList.Add(field.DisplayName);
                ColumnIndices[field.DisplayName] = Int32.Parse(field.FieldIdentifier);
                Dt.Columns.Add(field.DisplayName);
            }

            //TODO: clean up, use Linq
            var requestedFieldCount = fields.Count();
            foreach (var entry in entryIds)
            {
                var dt = Dt.NewRow();
                SeqLogger.Info("Row: {RowData}", entry);
                var newEntry = new string[requestedFieldCount];
                var data = entry.Split(',');
                for (var i = 0; i < requestedFieldCount; i++)
                {
                    dt[i] = data[ColumnIndices[requestedFieldList[i]]];
                    SeqLogger.Info("Value {i} set to {value}", i, dt[i]);
                }
                //SeqLogger.Info("Adding an entry: {Joined}", string.Join(",", (string[])dt.ItemArray));
                Dt.Rows.Add(dt);
            }

            return Dt.CreateDataReader();
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            SeqLogger.Info("GetFields...");
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
                SeqLogger.Info("\tAdding field {DisplayName}, {FieldIdentifer}", fieldName, idx.ToString());
                idx++;
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
