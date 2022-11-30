using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Reads data from multiple feeds in source workspace in batches, presenting it as if
    /// it were in one data layer in one batch.
    /// </summary>
    internal sealed class SourceWorkspaceDataReader : ISourceWorkspaceDataReader
    {
        private FieldInfoDto _identifierField;
        private IBatchDataReader _currentReader;

        private long _completedItem = 0;

        private readonly IRelativityExportBatcher _exportBatcher;
        private readonly IFieldManager _fieldManager;
        private readonly IAPILog _logger;
        private readonly ISynchronizationConfiguration _configuration;
        private readonly IBatchDataReaderBuilder _readerBuilder;
        private readonly CancellationToken _cancellationToken;

        public event OnSourceWorkspaceDataItemReadErrorEventHandler OnItemReadError;

        public SourceWorkspaceDataReader(IBatchDataReaderBuilder readerBuilder,
            ISynchronizationConfiguration configuration,
            IRelativityExportBatcher exportBatcher,
            IFieldManager fieldManager,
            IItemStatusMonitor itemStatusMonitor,
            IAPILog logger,
            CancellationToken cancellationToken)
        {
            _readerBuilder = readerBuilder;
            _readerBuilder.ItemLevelErrorHandler = RaiseOnItemReadError;

            _configuration = configuration;
            _exportBatcher = exportBatcher;
            _fieldManager = fieldManager;
            _logger = logger;
            _cancellationToken = cancellationToken;

            ItemStatusMonitor = itemStatusMonitor;

            _currentReader = EmptyDataReader();
        }

        private FieldInfoDto IdentifierField
        {
            get
            {
                if (_identifierField is null)
                {
                    _identifierField = _fieldManager.GetObjectIdentifierFieldAsync(CancellationToken.None).GetAwaiter().GetResult();
                }

                return _identifierField;
            }
        }

        public IItemStatusMonitor ItemStatusMonitor { get; }

        public bool Read()
        {
            if (_cancellationToken.IsCancellationRequested && _currentReader.CanCancel)
            {
                _currentReader.Dispose();
                return false;
            }

            bool dataRead = _currentReader.Read();
            if (!dataRead)
            {
                _currentReader.Dispose();
                _currentReader = GetReaderForNextBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                dataRead = _currentReader.Read();
            }

            if (dataRead)
            {
                string itemIdentifier = _currentReader[IdentifierField.DestinationFieldName].ToString();
                ItemStatusMonitor.MarkItemAsRead(itemIdentifier);
                _completedItem++;
            }
            else
            {
                _logger.LogInformation("No more data to be read from source workspace.");
            }

            return dataRead;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private IBatchDataReader EmptyDataReader()
        {
            _logger.LogInformation("Creating empty data reader.");
            return new EmptyBatchDataReader();
        }

        private async Task<IBatchDataReader> GetReaderForNextBatchAsync()
        {
            RelativityObjectSlim[] batch;
            try
            {
                batch = await _exportBatcher.GetNextItemsFromBatchAsync().ConfigureAwait(false);
                _logger.LogInformation("Export API batch read. Items count: {count}", batch?.Length);
            }
            catch (Exception ex)
            {
                const string message = "Failed to get next batch from exporter.";
                _logger.LogError(ex, message);
                throw new SourceDataReaderException(message, ex);
            }

            IBatchDataReader nextBatchReader;
            if (batch == null || !batch.Any())
            {
                _logger.LogInformation("Batch returned from Export API is empty.");
                nextBatchReader = EmptyDataReader();
            }
            else
            {
                try
                {
                    CreateItemStatusRecords(batch);
                    nextBatchReader = await _readerBuilder.BuildAsync(_configuration.SourceWorkspaceArtifactId, batch, _cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Created DataReader for next Export API batch.");
                }
                catch (Exception ex)
                {
                    const string message = "Failed to prepare exported batch for import";
                    _logger.LogError(ex, message);
                    throw new SourceDataReaderException(message, ex);
                }
            }

            return nextBatchReader;
        }

        private void CreateItemStatusRecords(RelativityObjectSlim[] batch)
        {
            foreach (RelativityObjectSlim item in batch)
            {
                int documentFieldIndex = IdentifierField.DocumentFieldIndex;
                string itemIdentifier = item.Values[documentFieldIndex].ToString();
                int itemArtifactId = item.ArtifactID;
                ItemStatusMonitor.AddItem(itemIdentifier, itemArtifactId);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _currentReader != null)
            {
                _currentReader.Dispose();
                _currentReader = null;
            }
        }

        private void RaiseOnItemReadError(string itemIdentifier, string message)
        {
            _completedItem++;

            // Logging and marking item as failed is happening in ImportJob.HandleItemLevelError
            OnItemReadError?.Invoke(_completedItem, new ItemLevelError(itemIdentifier, message));
        }

        #region Pass-thrus to _currentBatch

        public string GetName(int i)
        {
            return _currentReader.GetName(i);
        }

        public string GetDataTypeName(int i)
        {
            return _currentReader.GetDataTypeName(i);
        }

        public Type GetFieldType(int i)
        {
            return _currentReader.GetFieldType(i);
        }

        public object GetValue(int i)
        {
            return _currentReader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return _currentReader.GetValues(values);
        }

        public int GetOrdinal(string name)
        {
            return _currentReader.GetOrdinal(name);
        }

        public bool GetBoolean(int i)
        {
            return _currentReader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _currentReader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _currentReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _currentReader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _currentReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public Guid GetGuid(int i)
        {
            return _currentReader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _currentReader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _currentReader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _currentReader.GetInt64(i);
        }

        public float GetFloat(int i)
        {
            return _currentReader.GetFloat(i);
        }

        public double GetDouble(int i)
        {
            return _currentReader.GetDouble(i);
        }

        public string GetString(int i)
        {
            return _currentReader.GetString(i);
        }

        public decimal GetDecimal(int i)
        {
            return _currentReader.GetDecimal(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _currentReader.GetDateTime(i);
        }

        public IDataReader GetData(int i)
        {
            return _currentReader.GetData(i);
        }

        public bool IsDBNull(int i)
        {
            return _currentReader.IsDBNull(i);
        }

        public int FieldCount => _currentReader.FieldCount;

        public object this[int i] => _currentReader[i];

        public object this[string name] => _currentReader[name];

        public bool IsClosed => _currentReader.IsClosed;

        public void Close()
        {
            _currentReader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return _currentReader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return _currentReader.NextResult();
        }

        public int Depth => _currentReader.Depth;

        public int RecordsAffected => _currentReader.RecordsAffected;

        #endregion

    }
}
