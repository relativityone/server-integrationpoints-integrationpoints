using System;
using System.Data;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Reads data from multiple feeds in source workspace in batches, presenting it as if
	/// it were in one data layer in one batch.
	/// </summary>
	internal sealed class SourceWorkspaceDataReader : IDataReader
	{
		private IDataReader _currentReader;
		private Guid? _batchToken;

		private readonly IRelativityExportBatcher _exportBatcher;
		private readonly IFieldManager _fieldManager;
		private readonly IItemStatusMonitor _itemStatusMonitor;
		private readonly ISyncLog _logger;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly IBatchDataReaderBuilder _readerBuilder;

		public SourceWorkspaceDataReader(IBatchDataReaderBuilder readerBuilder,
			ISynchronizationConfiguration configuration,
			IRelativityExportBatcher exportBatcher,
			IFieldManager fieldManager,
			IItemStatusMonitor itemStatusMonitor,
			ISyncLog logger)
		{
			_readerBuilder = readerBuilder;
			_exportBatcher = exportBatcher;
			_fieldManager = fieldManager;
			_itemStatusMonitor = itemStatusMonitor;
			_logger = logger;
			_configuration = configuration;

			_currentReader = EmptyDataReader();
			_batchToken = null;
		}

		public bool Read()
		{
			bool dataRead = _currentReader.Read();
			if (!dataRead)
			{
				_currentReader.Dispose();
				_currentReader = GetReaderForNextBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				dataRead = _currentReader.Read();
			}

			if (dataRead)
			{
				string identifierFieldName = _fieldManager.GetObjectIdentifierFieldAsync(CancellationToken.None).GetAwaiter().GetResult().DisplayName;
				string itemIdentifier = _currentReader[identifierFieldName].ToString();
				_itemStatusMonitor.MarkItemAsRead(itemIdentifier);
			}
			return dataRead;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private static IDataReader EmptyDataReader() => new DataTable().CreateDataReader();

		private async Task<IDataReader> GetReaderForNextBatchAsync()
		{
			if (!_batchToken.HasValue)
			{
				_batchToken = _exportBatcher.Start(_configuration.ExportRunId, _configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId);
			}

			RelativityObjectSlim[] batch;
			try
			{
				batch = await _exportBatcher.GetNextAsync(_batchToken.Value).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new SourceDataReaderException("Failed to get next batch from exporter", ex);
			}

			IDataReader nextBatchReader;
			if (batch == null || !batch.Any())
			{
				nextBatchReader = EmptyDataReader();
			}
			else
			{
				await CreateItemStatusRecords(batch).ConfigureAwait(false);
				try
				{
					nextBatchReader = await _readerBuilder.BuildAsync(_configuration.SourceWorkspaceArtifactId, batch, CancellationToken.None).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SourceDataReaderException("Failed to prepare exported batch for import", ex);
				}
			}

			return nextBatchReader;
		}

		private async Task CreateItemStatusRecords(RelativityObjectSlim[] batch)
		{
			foreach (var item in batch)
			{
				FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(CancellationToken.None).ConfigureAwait(false);
				int documentFieldIndex = identifierField.DocumentFieldIndex;
				string itemIdentifier = item.Values[documentFieldIndex].ToString();
				int itemArtifactId = item.ArtifactID;
				_itemStatusMonitor.AddItem(itemIdentifier, itemArtifactId);
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
