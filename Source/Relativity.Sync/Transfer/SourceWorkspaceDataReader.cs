using System;
using System.Data;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Reads data from multiple feeds in source workspace in batches, presenting it as if
	/// it were in one data layer in one batch.
	/// </summary>
	internal sealed class SourceWorkspaceDataReader : IDataReader
	{
		private IDataReader _currentBatch;

		// TODO: Get rid of me; you should be using something like IBatchRepository.
		private int _currentIndex;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly ISourceWorkspaceDataTableBuilder _tableBuilder;
		private readonly int _resultsBlockSize;

		public SourceWorkspaceDataReader(ISourceServiceFactoryForUser serviceFactory,
			ISourceWorkspaceDataTableBuilder tableBuilder,
			ISyncLog logger,
			ISynchronizationConfiguration configuration,
			int resultsBlockSize)
		{
			_resultsBlockSize = resultsBlockSize;
			_serviceFactory = serviceFactory;
			_tableBuilder = tableBuilder;
			_logger = logger;
			_configuration = configuration;
			_currentBatch = new DataTable().CreateDataReader();

			_currentIndex = 0;
		}

		public bool IsClosed { get; } = false;

		public bool Read()
		{
			bool dataRead = _currentBatch.Read();
			if (!dataRead)
			{
				ReadNextBatchAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				dataRead = _currentBatch.Read();
			}

			return dataRead;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private async Task ReadNextBatchAsync()
		{
			RelativityObjectSlim[] block = await RetrieveDataBlock().ConfigureAwait(false);

			_currentIndex += block.Length;

			DataTable dt = await _tableBuilder.BuildAsync(_configuration.SourceWorkspaceArtifactId, _configuration.FieldMappings, block).ConfigureAwait(false);
			_currentBatch = dt.CreateDataReader();
		}

		private async Task<RelativityObjectSlim[]> RetrieveDataBlock()
		{
			RelativityObjectSlim[] block;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				block = await objectManager
					.RetrieveResultsBlockFromExportAsync(_configuration.SourceWorkspaceArtifactId, _configuration.ExportRunId, _resultsBlockSize, _currentIndex).ConfigureAwait(false);
			}

			return block;
		}

		private void Dispose(bool disposing)
		{
			if (disposing && _currentBatch != null)
			{
				_currentBatch?.Dispose();
				_currentBatch = null;
			}
		}

		#region Pass-thrus to _currentBatch

		public string GetName(int i)
		{
			return _currentBatch.GetName(i);
		}

		public string GetDataTypeName(int i)
		{
			return _currentBatch.GetDataTypeName(i);
		}

		public Type GetFieldType(int i)
		{
			return _currentBatch.GetFieldType(i);
		}

		public object GetValue(int i)
		{
			return _currentBatch.GetValue(i);
		}

		public int GetValues(object[] values)
		{
			return _currentBatch.GetValues(values);
		}

		public int GetOrdinal(string name)
		{
			return _currentBatch.GetOrdinal(name);
		}

		public bool GetBoolean(int i)
		{
			return _currentBatch.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return _currentBatch.GetByte(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _currentBatch.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		public char GetChar(int i)
		{
			return _currentBatch.GetChar(i);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return _currentBatch.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		public Guid GetGuid(int i)
		{
			return _currentBatch.GetGuid(i);
		}

		public short GetInt16(int i)
		{
			return _currentBatch.GetInt16(i);
		}

		public int GetInt32(int i)
		{
			return _currentBatch.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return _currentBatch.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			return _currentBatch.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			return _currentBatch.GetDouble(i);
		}

		public string GetString(int i)
		{
			return _currentBatch.GetString(i);
		}

		public decimal GetDecimal(int i)
		{
			return _currentBatch.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			return _currentBatch.GetDateTime(i);
		}

		public IDataReader GetData(int i)
		{
			return _currentBatch.GetData(i);
		}

		public bool IsDBNull(int i)
		{
			return _currentBatch.IsDBNull(i);
		}

		public int FieldCount => _currentBatch.FieldCount;

		public object this[int i] => _currentBatch[i];

		public object this[string name] => _currentBatch[name];

		public void Close()
		{
			_currentBatch.Close();
		}

		public DataTable GetSchemaTable()
		{
			return _currentBatch.GetSchemaTable();
		}

		public bool NextResult()
		{
			return _currentBatch.NextResult();
		}

		public int Depth => _currentBatch.Depth;

		public int RecordsAffected => _currentBatch.RecordsAffected;

		#endregion
	}
}
