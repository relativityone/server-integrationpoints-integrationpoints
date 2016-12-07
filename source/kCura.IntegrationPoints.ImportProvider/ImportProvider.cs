using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
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
            //Get extracted text & native fields for relative path modifications
            ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
            string loadFileDir = Path.GetDirectoryName(settings.LoadFile);
            bool extractedTextHasPathInfo = !string.IsNullOrEmpty(settings.ExtractedTextPathFieldIdentifier);
            bool nativeFileHasPathInfo = !string.IsNullOrEmpty(settings.NativeFilePathFieldIdentifier);

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
                    string colValue = sourceRow[Int32.Parse(field.FieldIdentifier)];
                    if (((extractedTextHasPathInfo && field.FieldIdentifier == settings.ExtractedTextPathFieldIdentifier)
                        || (nativeFileHasPathInfo && field.FieldIdentifier == settings.NativeFilePathFieldIdentifier))
                        && !Path.IsPathRooted(colValue)) //Do not rewrite paths if column contains full path info
                    {
                        dtRow[field.FieldIdentifier] = Path.Combine(loadFileDir, colValue);
                    }
                    else
                    {
                        dtRow[field.FieldIdentifier] = colValue;
                    }
                }
                dt.Rows.Add(dtRow);
            }

            return dt.CreateDataReader();
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            IFieldParser parser = _fieldParserFactory.GetFieldParser(options);
            List<FieldEntry> result = new List<FieldEntry>();
            int idx = 0;
            foreach (string fieldName in parser.GetFields())
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
