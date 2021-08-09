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
		private readonly IExportDataSanitizer _dataSanitizer;

		public SourceWorkspaceDataReaderFactory(IRelativityExportBatcherFactory exportBatcherFactory, IFieldManager fieldManager, ISynchronizationConfiguration configuration,
			IExportDataSanitizer dataSanitizer, ISyncLog logger)
		{
			_exportBatcherFactory = exportBatcherFactory;
			_fieldManager = fieldManager;
			_configuration = configuration;
			_dataSanitizer = dataSanitizer;
			_logger = logger;
		}

		public ISourceWorkspaceDataReader CreateNativeSourceWorkspaceDataReader(IBatch batch, CancellationToken token)
		{
			return CreateSourceWorkspaceDataReader(batch, new NativeBatchDataReaderBuilder(_fieldManager, _dataSanitizer, _logger), token);
		}

		public ISourceWorkspaceDataReader CreateImageSourceWorkspaceDataReader(IBatch batch, CancellationToken token)
		{
			return CreateSourceWorkspaceDataReader(batch, new ImageBatchDataReaderBuilder(_fieldManager, _dataSanitizer, _logger), token);
		}

		private ISourceWorkspaceDataReader CreateSourceWorkspaceDataReader(IBatch batch, IBatchDataReaderBuilder batchDataReaderBuilder, CancellationToken token)
		{
			IRelativityExportBatcher relativityExportBatcher = _exportBatcherFactory.CreateRelativityExportBatcher(batch);
			return new SourceWorkspaceDataReader(batchDataReaderBuilder, _configuration, relativityExportBatcher, _fieldManager, new ItemStatusMonitor(), _logger, token);
		}
	}
}