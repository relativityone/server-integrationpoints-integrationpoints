using System.Collections;
using System.Diagnostics.CodeAnalysis;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	[ExcludeFromCodeCoverage]
	internal sealed class SyncImportBulkArtifactJob : ISyncImportBulkArtifactJob
	{
		private const string _IAPI_IDENTIFIER_COLUMN = "Identifier";
		private const string _IAPI_MESSAGE_COLUMN = "Message";

		private readonly ImportBulkArtifactJob _importBulkArtifactJob;

		public SyncImportBulkArtifactJob(ImportBulkArtifactJob importBulkArtifactJob, ISourceWorkspaceDataReader sourceWorkspaceDataReader)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_importBulkArtifactJob.OnProgress += RaiseOnProgress;
			_importBulkArtifactJob.OnError += HandleIapiItemLevelError;

			ItemStatusMonitor = sourceWorkspaceDataReader.ItemStatusMonitor;
			sourceWorkspaceDataReader.OnItemReadError += HandleSourceWorkspaceDataItemReadError;
		}

		public IItemStatusMonitor ItemStatusMonitor { get; }

		public event IImportNotifier.OnCompleteEventHandler OnComplete
		{
			add => _importBulkArtifactJob.OnComplete += value;
			remove => _importBulkArtifactJob.OnComplete -= value;
		}

		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException
		{
			add => _importBulkArtifactJob.OnFatalException += value;
			remove => _importBulkArtifactJob.OnFatalException -= value;
		}

		public event IImportNotifier.OnProgressEventHandler OnProgress;

		public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress
		{
			add => _importBulkArtifactJob.OnProcessProgress += value;
			remove => _importBulkArtifactJob.OnProcessProgress -= value;
		}

		public event OnSyncImportBulkArtifactJobItemLevelErrorEventHandler OnItemLevelError;

		public void Execute()
		{
			_importBulkArtifactJob.Execute();
		}

		private void RaiseOnProgress(long completedRow)
		{
			OnProgress?.Invoke(completedRow);
		}

		private void RaiseOnItemLevelError(ItemLevelError itemLevelError)
		{
			OnItemLevelError?.Invoke(itemLevelError);
		}

		private void HandleIapiItemLevelError(IDictionary row)
		{
			RaiseOnItemLevelError(new ItemLevelError(
				GetValueOrNull(row, _IAPI_IDENTIFIER_COLUMN),
				$"IAPI {GetValueOrNull(row, _IAPI_MESSAGE_COLUMN)}"
				));
		}

		private void HandleSourceWorkspaceDataItemReadError(long completedItem, ItemLevelError itemLevelError)
		{
			RaiseOnProgress(completedItem);

			RaiseOnItemLevelError(itemLevelError);
		}

		private static string GetValueOrNull(IDictionary row, string key)
		{
			return row.Contains(key) ? row[key].ToString() : null;
		}
	}
}