using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class FileDataConverter : IDataSyncronizer
	{
		private readonly string _outputFile;
		private static int _index = 0;


		public FileDataConverter(string outputFile)
		{
			_outputFile = outputFile;
		}

		private void ProcessRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> map, StreamWriter writer)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("obj {0}\n", _index);
			foreach (var fieldMap in map)
			{
				var str = string.Format("{0}: {1}\n", fieldMap.DestinationField.FieldIdentifier, row[fieldMap.SourceField]);
				builder.Append(str);
			}
			writer.Write(builder.ToString());
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap)
		{
			var map = fieldMap.ToList();

			using (var file = File.AppendText(_outputFile))
			{
				foreach (var dataRow in data)
				{
					ProcessRow(dataRow, map, file);
					_index++;
				}
			}
		}
	}
}
