using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using kCura.IntegrationPoints.Contracts.Models;

namespace Provider
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider(GlobalConstants.FIRST_PROVIDER_GUID)]
	public class MyFirstProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			string fileLocation = options;
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

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			string fileLocation = options;
			return GetDataReader(fileLocation);
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			string fileLocation = options;
			return GetDataReader(fileLocation);
		}

		private IDataReader GetDataReader(string fileLocation)
		{
			var dt = new DataTable();
			XmlDocument doc = new XmlDocument();
			doc.Load(fileLocation);
			XmlNodeList nodes = doc.DocumentElement.SelectNodes("/root/data/document");
			var fields = this.GetFields(fileLocation);
			foreach (var field in fields)
			{
				dt.Columns.Add(field.FieldIdentifier);
			}
			foreach (XmlNode node in nodes)
			{
				var row = dt.NewRow();
				foreach (var fieldEntry in fields)
				{
					row[fieldEntry.FieldIdentifier] = node.SelectSingleNode(fieldEntry.FieldIdentifier).InnerText;
				}
				dt.Rows.Add(row);
			}
			return dt.CreateDataReader();
		}


	}
}
