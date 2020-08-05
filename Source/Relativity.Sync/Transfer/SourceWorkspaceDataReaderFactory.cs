using System.Threading;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceWorkspaceDataReaderFactory : ISourceWorkspaceDataReaderFactory
	{
		private readonly IRelativityExportBatcherFactory _exportBatcherFactory;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly IBatchDataReaderBuilder _readerBuilder;

		public SourceWorkspaceDataReaderFactory(IRelativityExportBatcherFactory exportBatcherFactory, IFieldManager fieldManager, ISynchronizationConfiguration configuration, 
			IBatchDataReaderBuilder readerBuilder, ISyncLog logger)
		{
			_exportBatcherFactory = exportBatcherFactory;
			_fieldManager = fieldManager;
			_configuration = configuration;
			_readerBuilder = readerBuilder;
			_logger = logger;
		}

		public ISourceWorkspaceDataReader CreateSourceWorkspaceDataReader(IBatch batch, CancellationToken token)
		{
			IRelativityExportBatcher relativityExportBatcher = _exportBatcherFactory.CreateRelativityExportBatcher(batch);
			return new SourceWorkspaceDataReader(_readerBuilder, _configuration, relativityExportBatcher, _fieldManager, new ItemStatusMonitor(), _logger, token);
		}
	}
}