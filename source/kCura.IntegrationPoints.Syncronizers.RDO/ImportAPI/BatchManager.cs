using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class BatchManager
	{
		internal List<IDictionary<string, object>> _dataSource;
		private readonly List<string> _columnNames;
		private readonly int _minBatchSize;

		public event BatchCreated OnBatchCreate;

		public BatchManager(int minBatchSize = 1000)
		{
			_dataSource = new List<IDictionary<string, object>>();
			_columnNames = new List<string>();
			_minBatchSize = minBatchSize;
		}

		public int MinimumBatchSize { get { return _minBatchSize; } }
		public int CurrentSize { get { return _dataSource.Count; } }

		public void Add(IDictionary<string, object> fileData)
		{
			_dataSource.Add(fileData);
			foreach (var key in fileData.Keys.Where(key => !_columnNames.Contains(key)))
			{
				_columnNames.Add(key);
			}
			if (_dataSource.Count == 1 && OnBatchCreate != null)
			{
				OnBatchCreate(_minBatchSize);
			}
		}

		public bool IsBatchFull()
		{
			return _dataSource.Count >= _minBatchSize;
		}

		public IDataReader GetBatchData()
		{
			IDataReader importDataReader = null;
			DataTable dt = ConfigureTable(_columnNames, _dataSource);
			if (dt.Rows.Count > 0)
			{
				importDataReader = new DataTableReader(dt);
			}
			return importDataReader;
		}

		public void ClearDataSource()
		{
			_dataSource.Clear();
		}

		/// <summary>
		/// Convert a list of strings to table columns
		/// </summary>
		/// <param name="columnNames"></param>
		/// <returns></returns>
		internal static DataTable ConfigureTable(IEnumerable<string> columnNames, List<IDictionary<string, object>> dataSource)
		{
			var finalDt = new DataTable();
			foreach (var column in columnNames)
			{
				finalDt.Columns.Add(column);
			}

			foreach (var dictionary in dataSource)
			{
				var row = finalDt.NewRow();
				foreach (var kvp in dictionary)
				{
					row[kvp.Key] = kvp.Value;
				}
				finalDt.Rows.Add(row);
			}

			return finalDt;
		}
	}
}