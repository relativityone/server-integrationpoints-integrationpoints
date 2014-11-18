using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	public class Manager
	{
		private readonly IDataSourceFactory _factory;

		private int ChunkSize
		{
			get { return 2; }
		}

		public Manager(IDataSourceFactory dataSourceFactory)
		{
			_factory = dataSourceFactory;
		}

		private FieldEntry GetFieldIdentifierEntry()
		{
			return new FieldEntry { DisplayName = "display", FieldIdentifier = "name", FieldType = FieldType.String };
		}

		private string GetSourceConfig()
		{
			return string.Empty;
		}
		public void Execute()
		{
			IDataSourceProvider dataSource = _factory.GetDataSource();
			FieldEntry identifier = GetFieldIdentifierEntry();
			var config = GetSourceConfig();
			IDataReader readerToBatch = dataSource.GetBatchableData(identifier, config);

			CreateBatchJobs(readerToBatch);

		}

		private void CreateJob(IEnumerable<string> ids)
		{
			Data.BatchTable.Push(ids);
		}
		//TODO abstract create batch
		private void CreateBatchJobs(IDataReader reader)
		{
			var max = 1000;
			var list = new List<string>(max);
			var idx = 0;
			while (reader.Read())
			{
				list.Add(reader.GetString(0));
				if (idx == max)
				{
					CreateJob(list);
					list.Clear();
					idx = 0;
				}
				idx++;
			}
		}

	}
}
