using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System
{
    internal class DataReaderRowSetValidator : IDataReaderRowSetValidator
    {
        private readonly IDictionary<string, DataRow> _dataRows;
        private readonly IDictionary<string, bool> _registered;

        private DataReaderRowSetValidator(IDictionary<string, DataRow> dataRows, IDictionary<string, bool> registered)
        {
            _dataRows = dataRows;
            _registered = registered;
        }

        public static DataReaderRowSetValidator Create(DataTable dataTable)
        {
            IDictionary<string, DataRow> dataRows = BuildDataRows(dataTable);
            IDictionary<string, bool> registered = BuildRegistered(dataTable);

            return new DataReaderRowSetValidator(dataRows, registered);
        }

        private static IDictionary<string, DataRow> BuildDataRows(DataTable dataTable)
        {
            var dictionary = new Dictionary<string, DataRow>();

            foreach (DataRow dataRow in dataTable.Rows)
            {
                string identifier = (string)dataRow[ImportDataTableWrapper.IdentifierFieldName];
                dictionary.Add(identifier, dataRow);
            }

            return dictionary;
        }

        private static IDictionary<string, bool> BuildRegistered(DataTable data)
        {
            var dictionary = new Dictionary<string, bool>();

            foreach (DataRow dataRow in data.Rows)
            {
                string identifier = (string)dataRow[ImportDataTableWrapper.IdentifierFieldName];
                dictionary.Add(identifier, false);
            }

            return dictionary;
        }

        public void ValidateAndRegisterRead(string controlNumber, params FieldVerifyData[] fieldVerifyData)
        {
            DataRow row = _dataRows[controlNumber];

            foreach (FieldVerifyData field in fieldVerifyData)
            {
                object expectedValue = row[field.ColumnName];
                field.Validator(controlNumber, expectedValue, field.ActualValue);
            }

            _registered[controlNumber] = true;
        }

        public void ValidateAllRead()
        {
            Assert.That(_registered.All(pair => pair.Value), GetVerifyAllFailureMessage);
        }

        private IEnumerable<string> GetUnverifiedDocumentIds()
        {
            return _registered
                .Where(x => x.Value == false)
                .Select(x => x.Key);
        }

        private string GetVerifyAllFailureMessage()
        {
            IEnumerable<string> unverifiedDocumentIds = GetUnverifiedDocumentIds();
            return $"Missing documents: {string.Join(", ", unverifiedDocumentIds)}";
        }
    }
}