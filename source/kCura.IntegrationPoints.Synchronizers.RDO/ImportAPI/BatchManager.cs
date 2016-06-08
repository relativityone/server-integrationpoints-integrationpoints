using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
    public class BatchManager
    {
        internal List<IDictionary<string, object>> _dataSource;
        private HashSet<string> _columnNames;
        private readonly int _minBatchSize;

        public event BatchCreated OnBatchCreate;

        public BatchManager(int minBatchSize = 1000)
        {
            _dataSource = new List<IDictionary<string, object>>();
            _minBatchSize = minBatchSize;
        }

        public int MinimumBatchSize { get { return _minBatchSize; } }
        public int CurrentSize { get { return _dataSource.Count; } }

        public HashSet<string> ColumnNames
        {
            get { return _columnNames; }
            set { _columnNames = value; }
        }

        public void Add(IDictionary<string, object> fileData)
        {
            _dataSource.Add(fileData);
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
            DataTable dt = ConfigureTable(ColumnNames, _dataSource);
            if (dt != null)
            {
                importDataReader = dt.CreateDataReader();
            }
            return importDataReader;
        }

        public void ClearDataSource()
        {
            _dataSource.Clear();
        }

        /// <summary>
        /// Convert a list of data rows to data table
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public DataTable ConfigureTable(IEnumerable<string> columnNames, List<IDictionary<string, object>> dataSource)
        {
            DataTable finalDt = new DataTable();
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
            if (finalDt.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return finalDt;
            }
        }
    }
}