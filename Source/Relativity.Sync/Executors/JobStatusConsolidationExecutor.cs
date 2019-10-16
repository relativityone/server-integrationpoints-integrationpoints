﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal class JobStatusConsolidationExecutor : IExecutor<IJobStatusConsolidationConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		private static readonly Guid CompletedItemsCountGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
		private static readonly Guid FailedItemsCountGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
		private static readonly Guid TotalItemsCountGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

		public JobStatusConsolidationExecutor(IBatchRepository batchRepository, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IJobStatusConsolidationConfiguration configuration, CancellationToken token)
		{
			List<IBatch> batches = (await _batchRepository
				.GetAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId)
				.ConfigureAwait(false)).ToList();

			int completedRecordsCount = batches
				.Sum(batch => batch.TransferredItemsCount);

			int totalItemsCount = batches
				.Sum(batch => batch.TotalItemsCount);

			int failedRecordsCount = batches
				.Sum(batch => batch.FailedItemsCount);

			UpdateResult updateResult;
			try
			{
				updateResult = await UpdateJobHistoryAsync(configuration, completedRecordsCount, failedRecordsCount, totalItemsCount)
					.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				return ExecutionResult.Failure("Failed to update Job History object.", e);
			}

			if (updateResult.EventHandlerStatuses.Any(status => !status.Success))
			{
				IEnumerable<string> eventHandlerMessages = updateResult.EventHandlerStatuses.Select(status => status.Message);
				string joinedEventHandlerMessages = string.Join(", ", eventHandlerMessages);
				string message = $"Event handlers failed when updating Job History object: {joinedEventHandlerMessages}.";
				return ExecutionResult.Failure(message, null);
			}

			return ExecutionResult.Success();
		}

		private async Task<UpdateResult> UpdateJobHistoryAsync(IJobStatusConsolidationConfiguration configuration, int completedRecordsCount, int failedRecordsCount, int totalItemsCount)
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var updateRequest = new UpdateRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = configuration.JobHistoryArtifactId
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = CompletedItemsCountGuid
							},
							Value = completedRecordsCount
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = FailedItemsCountGuid
							},
							Value = failedRecordsCount
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = TotalItemsCountGuid
							},
							Value = totalItemsCount
						}
					}
				};

				return await objectManager
					.UpdateAsync(configuration.SourceWorkspaceArtifactId, updateRequest)
					.ConfigureAwait(false);
			}
		}
	}
}