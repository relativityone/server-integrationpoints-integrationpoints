using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
    public class BatchManager
    {
        private readonly int _minBatchSize;
        internal List<IDictionary<string, object>> _dataSource;

        public event BatchCreated OnBatchCreate;

        public BatchManager(int minBatchSize = 1000)
        {
            _dataSource = new List<IDictionary<string, object>>();
            _minBatchSize = minBatchSize;
        }

        public int MinimumBatchSize => _minBatchSize;

        public int CurrentSize => _dataSource.Count;

        public HashSet<string> ColumnNames { get; set; }

        public void Add(IDictionary<string, object> fileData)
        {
            _dataSource.Add(fileData);
            if (_dataSource.Count == 1)
            {
                OnBatchCreate?.Invoke(_minBatchSize);
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
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public DataTable ConfigureTable(IEnumerable<string> columnNames, List<IDictionary<string, object>> dataSource)
        {
            DataTable finalDt = new DataTable();
            foreach (var column in columnNames)
            {
                finalDt.Columns.Add(column);
            }

            // Non Relativity providers will not have the Native File Path column passed in.
            // We must add it here for them.
            bool? dataSourceContainsNativeFilePath = dataSource.FirstOrDefault()?.Keys.Contains(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME);
            if (dataSourceContainsNativeFilePath.HasValue
                && dataSourceContainsNativeFilePath.Value
                && !finalDt.Columns.Contains(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME))
            {
                finalDt.Columns.Add(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME);
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
