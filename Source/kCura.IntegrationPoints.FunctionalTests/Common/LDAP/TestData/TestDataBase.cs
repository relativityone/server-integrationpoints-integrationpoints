using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
    public abstract class TestDataBase
    {
        private const string _RELATIVE_TEST_DATA_PATH = @"Common\LDAP\TestData";
        private readonly string _TEST_DATA_PATH = Path.Combine(
            new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
            _RELATIVE_TEST_DATA_PATH);

        public readonly string UniqueId;

        public readonly IList<IDictionary<string, object>> Data;

        public abstract string OU { get; }

        public FieldEntry IdentifierFieldEntry => new FieldEntry
        {
            DisplayName = UniqueId,
            FieldIdentifier = UniqueId,
            IsIdentifier = true
        };

        public string[] AllProperties { get; protected set; }

        protected TestDataBase(string testDataName, string uniqueId)
        {
            string testDataFile = Path.Combine(_TEST_DATA_PATH, $"{testDataName}.csv");

            TextFieldParser csvParser = new TextFieldParser(testDataFile)
            {
                Delimiters = new[] { "," }
            };

            AllProperties = csvParser.ReadFields();

            Data = new List<IDictionary<string, object>>();
            while (!csvParser.EndOfData)
            {
                string[] fields = csvParser.ReadFields();

                Dictionary<string, object> row = AllProperties.Zip(fields, (prop, value) => new { prop, value })
                    .ToDictionary(x => x.prop, x => string.IsNullOrEmpty(x.value) ? null : (object)x.value);

                Data.Add(row);
            }

            UniqueId = uniqueId;
        }

        public IEnumerable<FieldEntry> GetFieldEntries()
        {
            var entries = AllProperties.Select(x => new FieldEntry() { DisplayName = x, FieldIdentifier = x }).ToList();

            entries.Single(x => x.FieldIdentifier == UniqueId).IsIdentifier = true;

            return entries;
        }

        public virtual IEnumerable<string> EntryIds => Data.Select(x => x[UniqueId].ToString());
    }
}
