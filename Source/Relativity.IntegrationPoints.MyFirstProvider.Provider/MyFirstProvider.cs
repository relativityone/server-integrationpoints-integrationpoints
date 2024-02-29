using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider
{
    /// <summary>
    /// This code is a sample fully operational Integration Point Provider
    /// for demonstration purposes only
    /// </summary>
    [DataSourceProvider(GlobalConstants.FIRST_PROVIDER_GUID)]
    public class MyFirstProvider : IDataSourceProvider
    {
		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
            string fileLocation = providerConfiguration.Configuration;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileLocation);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/root/columns/column");
            var fieldEntries = new List<FieldEntry>();
            foreach (XmlNode node in nodes)
            {
                var field = node.InnerText;
                fieldEntries.Add(new FieldEntry { DisplayName = field, FieldIdentifier = field, IsIdentifier = field.Equals("Name") });
            }

            return fieldEntries;
        }

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
            string fileLocation = providerConfiguration.Configuration;

            DataTable dt = new DataTable();
            dt.Columns.Add(identifier.FieldIdentifier);

            XmlDocument doc = new XmlDocument();
            doc.Load(fileLocation);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes(string.Format("/root/data/document/{0}", identifier.FieldIdentifier));

            foreach (XmlNode node in nodes)
            {
                var row = dt.NewRow();
                row[identifier.FieldIdentifier] = node.InnerText;
                dt.Rows.Add(row);
            }
            return dt.CreateDataReader();
        }

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
            string fileLocation = providerConfiguration.Configuration;
            List<string> fieldList = fields.Select(f => f.FieldIdentifier).ToList();
            string keyFieldName = fields.FirstOrDefault(f => f.IsIdentifier).FieldIdentifier;
            return new XMLDataReader(entryIds, fieldList, keyFieldName, fileLocation);
        }
    }
}
