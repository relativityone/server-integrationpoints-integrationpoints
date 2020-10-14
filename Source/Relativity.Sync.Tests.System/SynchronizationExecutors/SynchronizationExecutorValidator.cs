﻿using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.System.SynchronizationExecutors
{
	internal class SynchronizationExecutorValidator
	{
		public ConfigurationStub Configuration { get; }
		public ServiceFactory ServiceFactory { get; }

		private readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private readonly Guid BatchObject = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private readonly Guid TransferredItemsCountField = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private readonly Guid SyncConfigurationRelation = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		public SynchronizationExecutorValidator(ConfigurationStub configuration, ServiceFactory serviceFactory)
		{
			Configuration = configuration;
			ServiceFactory = serviceFactory;
		}

		public void AssertResult(ExecutionResult result, ExecutionStatus expectedStatus)
		{
			Assert.AreEqual(expectedStatus, result.Status,
				message: AggregateJobHistoryErrorMessagesAsync(result).GetAwaiter().GetResult());
		}

		public void AssertTotalTransferredItems(int expectedTotalCount)
		{
			Assert.AreEqual(expectedTotalCount, 
				GetTotalTransferredItemsCountAsync().GetAwaiter().GetResult());
		}

		public void AssertTransferredItemsInBatches(IList<int> expectedTagCountInBatches)
		{
			IList<int> transferredItemsCountsPerBatch = GetTransferredItemsCountsPerBatchAsync().GetAwaiter().GetResult();

			CollectionAssert.IsNotEmpty(transferredItemsCountsPerBatch);
			CollectionAssert.AreEqual(transferredItemsCountsPerBatch, expectedTagCountInBatches);
		}

		#region Private Methods

		private async Task<string> AggregateJobHistoryErrorMessagesAsync(ExecutionResult syncResult)
		{
			IEnumerable<RelativityObject> jobHistoryErrors =
				await GetAllJobErrorsAsync().ConfigureAwait(false);

			var sb = new StringBuilder();
			sb.AppendLine($"Synchronization step failed: {syncResult.Message}: {syncResult.Exception}");
			foreach (RelativityObject err in jobHistoryErrors)
			{
				sb.AppendLine($"Item level error: {err[ErrorMessageField].Value}")
					.AppendLine((string)err[StackTraceField].Value)
					.AppendLine();
			}

			return sb.ToString();
		}

		private async Task<IEnumerable<RelativityObject>> GetAllJobErrorsAsync()
		{
			var serviceFactory = new ServiceFactoryStub(ServiceFactory);
			using (var objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Guid = JobHistoryErrorObject },
					Condition = $"'Job History' == {Configuration.JobHistoryArtifactId}",
					Fields = new List<FieldRef>
					{
						new FieldRef { Guid = ErrorMessageField },
						new FieldRef { Guid = StackTraceField }
					}
				};

				IEnumerable<QueryResult> results = await objectManager.QueryAllAsync(Configuration.SourceWorkspaceArtifactId, request).ConfigureAwait(false);

				return results.SelectMany(x => x.Objects);
			}
		}

		private async Task<int> GetTotalTransferredItemsCountAsync()
		{
			IList<int> batchesTransferredItemsCounts = await GetTransferredItemsCountsPerBatchAsync().ConfigureAwait(false);

			return batchesTransferredItemsCounts.Sum();
		}

		private async Task<IList<int>> GetTransferredItemsCountsPerBatchAsync()
		{
			List<int> batchesTransferredItemsCounts = new List<int>();

			var serviceFactory = new ServiceFactoryStub(ServiceFactory);

			using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var batchesArtifactsIdsQueryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObject
					},
					Condition = $"'{SyncConfigurationRelation}' == OBJECT {Configuration.SyncConfigurationArtifactId}"
				};

				QueryResultSlim batchesArtifactsIdsQueryResult = await objectManager
					.QuerySlimAsync(Configuration.SourceWorkspaceArtifactId, batchesArtifactsIdsQueryRequest, 1, int.MaxValue).ConfigureAwait(false);
				if (batchesArtifactsIdsQueryResult.TotalCount > 0)
				{
					IEnumerable<int> batchesArtifactsIds = batchesArtifactsIdsQueryResult.Objects.Select(x => x.ArtifactID);

					foreach (int batchArtifactId in batchesArtifactsIds)
					{
						QueryRequest transferredItemsCountQueryRequest = new QueryRequest
						{
							ObjectType = new ObjectTypeRef
							{
								Guid = BatchObject
							},
							Fields = new[]
							{
								new FieldRef
								{
									Guid = TransferredItemsCountField
								}
							},
							Condition = $"'ArtifactID' == {batchArtifactId}"
						};
						QueryResult transferredItemsCountQueryResult = await objectManager
							.QueryAsync(Configuration.SourceWorkspaceArtifactId, transferredItemsCountQueryRequest, 0, 1).ConfigureAwait(false);

						batchesTransferredItemsCounts.Add((int)(transferredItemsCountQueryResult.Objects.Single()[TransferredItemsCountField].Value ?? default(int)));
					};
				}
			}

			return batchesTransferredItemsCounts;
		}
	}

	#endregion
}
