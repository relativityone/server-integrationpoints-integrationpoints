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
	internal sealed class SourceWorkspaceDataReader : ISourceWorkspaceDataReader
	{
		private IDataReader _currentReader;

		private readonly IRelativityExportBatcher _exportBatcher;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly IBatchDataReaderBuilder _readerBuilder;
		private readonly CancellationToken _cancellationToken;

		public SourceWorkspaceDataReader(IBatchDataReaderBuilder readerBuilder,
			ISynchronizationConfiguration configuration,
			IRelativityExportBatcher exportBatcher,
			IFieldManager fieldManager,
			IItemStatusMonitor itemStatusMonitor, 
			ISyncLog logger,
			CancellationToken cancellationToken)
		{
			_readerBuilder = readerBuilder;
			_configuration = configuration;
			_exportBatcher = exportBatcher;
			_fieldManager = fieldManager;
			_logger = logger;
			_cancellationToken = cancellationToken;

			ItemStatusMonitor = itemStatusMonitor;

			_currentReader = EmptyDataReader();
		}

		public IItemStatusMonitor ItemStatusMonitor { get; }

		public bool Read()
		{
			if (_cancellationToken.IsCancellationRequested)
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
				string identifierFieldName = _fieldManager.GetObjectIdentifierFieldAsync(CancellationToken.None).GetAwaiter().GetResult().DestinationFieldName;
				string itemIdentifier = _currentReader[identifierFieldName].ToString();
				ItemStatusMonitor.MarkItemAsRead(itemIdentifier);
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

		private IDataReader EmptyDataReader()
		{
			_logger.LogVerbose("Creating empty data reader.");
			return new DataTable().CreateDataReader();
		}

		private async Task<IDataReader> GetReaderForNextBatchAsync()
		{
			RelativityObjectSlim[] batch;
			try
			{
				batch = await _exportBatcher.GetNextItemsFromBatchAsync().ConfigureAwait(false);
				_logger.LogVerbose("Export API batch read.");
			}
			catch (Exception ex)
			{
				const string message = "Failed to get next batch from exporter.";
				_logger.LogError(ex, message);
				throw new SourceDataReaderException(message, ex);
			}

			IDataReader nextBatchReader;
			if (batch == null || !batch.Any())
			{
				_logger.LogInformation("Batch returned from Export API is empty.");
				nextBatchReader = EmptyDataReader();
			}
			else
			{
				try
				{
					await CreateItemStatusRecordsAsync(batch).ConfigureAwait(false);
					nextBatchReader = await _readerBuilder.BuildAsync(_configuration.SourceWorkspaceArtifactId, batch, CancellationToken.None).ConfigureAwait(false);
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

		private async Task CreateItemStatusRecordsAsync(RelativityObjectSlim[] batch)
		{
			foreach (var item in batch)
			{
				FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(CancellationToken.None).ConfigureAwait(false);
				int documentFieldIndex = identifierField.DocumentFieldIndex;
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
