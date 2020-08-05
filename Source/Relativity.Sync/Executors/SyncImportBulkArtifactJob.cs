﻿using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
	[ExcludeFromCodeCoverage]
	internal sealed class SyncImportBulkArtifactJob : ISyncImportBulkArtifactJob
	{
		private int _sourceWorkspaceErrorItemsCount = 0;

		private const string _IAPI_IDENTIFIER_COLUMN = "Identifier";
		private const string _IAPI_MESSAGE_COLUMN = "Message";

		private readonly ImportBulkArtifactJob _importBulkArtifactJob;

		public SyncImportBulkArtifactJob(ImportBulkArtifactJob importBulkArtifactJob, ISourceWorkspaceDataReader sourceWorkspaceDataReader)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_importBulkArtifactJob.OnProgress += RaiseOnProgress;
			_importBulkArtifactJob.OnError += HandleIapiItemLevelError;
			_importBulkArtifactJob.OnComplete += HandleIapiJobComplete;
			_importBulkArtifactJob.OnFatalException += HandleIapiFatalException;

			ItemStatusMonitor = sourceWorkspaceDataReader.ItemStatusMonitor;
			sourceWorkspaceDataReader.OnItemReadError += HandleSourceWorkspaceDataItemReadError;
		}

		public IItemStatusMonitor ItemStatusMonitor { get; }

		public event SyncJobEventHandler<ItemLevelError> OnItemLevelError;
		public event SyncJobEventHandler<ImportApiJobProgress> OnProgress;
		public event SyncJobEventHandler<ImportApiJobStatistics> OnComplete;
		public event SyncJobEventHandler<ImportApiJobStatistics> OnFatalException;

		public void Execute()
		{
			_importBulkArtifactJob.Execute();
		}

		private void RaiseOnProgress(long completedRow)
		{
			OnProgress?.Invoke(new ImportApiJobProgress(completedRow));
		}

		private void HandleIapiItemLevelError(IDictionary row)
		{
			RaiseOnItemLevelError(new ItemLevelError(
				GetValueOrNull(row, _IAPI_IDENTIFIER_COLUMN),
				$"IAPI {GetValueOrNull(row, _IAPI_MESSAGE_COLUMN)}"
			));
		}

		private void HandleIapiJobComplete(JobReport jobReport)
		{
			OnComplete?.Invoke(CreateJobStatistics(jobReport));
		}

		private void HandleIapiFatalException(JobReport jobReport)
		{
			OnFatalException?.Invoke(CreateJobStatistics(jobReport));
		}

		private void RaiseOnItemLevelError(ItemLevelError itemLevelError)
		{
			OnItemLevelError?.Invoke(itemLevelError);
		}

		private void HandleSourceWorkspaceDataItemReadError(long completedItem, ItemLevelError itemLevelError)
		{
			_sourceWorkspaceErrorItemsCount++;

			RaiseOnProgress(completedItem);
			RaiseOnItemLevelError(itemLevelError);
		}

		private ImportApiJobStatistics CreateJobStatistics(JobReport jobReport)
		{
			ImportApiJobStatistics statistics = new ImportApiJobStatistics(
				jobReport.TotalRows + _sourceWorkspaceErrorItemsCount,
				jobReport.ErrorRowCount + _sourceWorkspaceErrorItemsCount,
				jobReport.MetadataBytes,
				jobReport.FileBytes,
				jobReport.FatalException
			);

			return statistics;
		}

		private static string GetValueOrNull(IDictionary row, string key)
		{
			return row.Contains(key) ? row[key].ToString() : null;
		}
	}
}