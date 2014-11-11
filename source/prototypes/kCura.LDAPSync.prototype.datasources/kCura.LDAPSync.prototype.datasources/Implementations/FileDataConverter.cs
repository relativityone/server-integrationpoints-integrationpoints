using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class FileDataConverter : IDataConverter
	{
		private readonly string _outputFile;
		private int _index;
		public FileDataConverter(string outputFile)
		{
			_outputFile = outputFile;
			_index = 0;
		}
		public void SyncData(IEnumerable<DataRow> data, IEnumerable<FieldMap> fieldMap)
		{
			var map = fieldMap.ToList();
			
			using (var file = new StreamWriter(_outputFile))
			{
				foreach (var dataRow in data)
				{
					ProcessRow(dataRow, map, file);
					_index++;
				}
			}
		}

		private void ProcessRow(DataRow row, IEnumerable<FieldMap> map, StreamWriter writer)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("obj {0}\n", _index);
			foreach (var fieldMap in map)
			{
				var str = string.Format("{0}: {1}\n", fieldMap.DestinationField, row[fieldMap.SourceField.FieldIdentifier]);
				builder.Append(str);
			}
			writer.Write(builder.ToString());
		}

	}
}
