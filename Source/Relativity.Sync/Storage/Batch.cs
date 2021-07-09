using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class Batch : IBatch
	{
		private readonly int _workspaceArtifactId;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		
		private static readonly Guid TotalItemsCountGuid = new Guid(SyncBatchGuids.TotalItemsCountGuid);
		private static readonly Guid StartingIndexGuid = new Guid(SyncBatchGuids.StartingIndexGuid);
		private static readonly Guid StatusGuid = new Guid(SyncBatchGuids.StatusGuid);
		private static readonly Guid FailedItemsCountGuid = new Guid(SyncBatchGuids.FailedItemsCountGuid);
		private static readonly Guid TransferredItemsCountGuid = new Guid(SyncBatchGuids.TransferredItemsCountGuid);
		private static readonly Guid TaggedItemsCountGuid = new Guid(SyncBatchGuids.TaggedItemsCountGuid);
		private static readonly Guid MetadataBytesTransferredGuid = new Guid(SyncBatchGuids.MetadataBytesTransferredGuid);
		private static readonly Guid FilesBytesTransferredGuid = new Guid(SyncBatchGuids.FilesBytesTransferredGuid);
		private static readonly Guid TotalBytesTransferredGuid = new Guid(SyncBatchGuids.TotalBytesTransferredGuid);
		private static readonly Guid ProgressGuid = new Guid(SyncBatchGuids.ProgressGuid);
		private static readonly Guid LockedByGuid = new Guid(SyncBatchGuids.LockedByGuid);

		internal const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";
		internal static readonly Guid BatchObjectTypeGuid = new Guid(SyncBatchGuids.SyncBatchObjectTypeGuid);

		private Batch(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public int ArtifactId { get; private set; }

		public int FailedItemsCount { get; private set; }

		public async Task SetFailedItemsCountAsync(int failedItemsCount)
		{
			await UpdateFieldValueAsync(FailedItemsCountGuid, failedItemsCount).ConfigureAwait(false);
			FailedItemsCount = failedItemsCount;
		}

		public int TransferredItemsCount { get; private set; }

		public async Task SetTransferredItemsCountAsync(int transferredItemsCount)
		{
			await UpdateFieldValueAsync(TransferredItemsCountGuid, transferredItemsCount).ConfigureAwait(false);
			TransferredItemsCount = transferredItemsCount;
		}

		public int TotalItemsCount { get; private set; }

		public long MetadataBytesTransferred { get; private set; }

		public async Task SetMetadataBytesTransferredAsync(long metadataBytesTransferred)
		{
			await UpdateFieldValueAsync(MetadataBytesTransferredGuid, metadataBytesTransferred).ConfigureAwait(false);
			MetadataBytesTransferred = metadataBytesTransferred;
		}

		public long FilesBytesTransferred { get; private set; }

		public async Task SetFilesBytesTransferredAsync(long filesBytesTransferred)
		{
			await UpdateFieldValueAsync(FilesBytesTransferredGuid, filesBytesTransferred).ConfigureAwait(false);
			FilesBytesTransferred = filesBytesTransferred;
		}

		public long TotalBytesTransferred { get; private set; }

		public async Task SetTotalBytesTransferredAsync(long totalBytesTransferred)
		{
			await UpdateFieldValueAsync(TotalBytesTransferredGuid, totalBytesTransferred).ConfigureAwait(false);
			TotalBytesTransferred = totalBytesTransferred;
		}

		public int StartingIndex { get; private set; }

		public string LockedBy { get; private set; }

		public async Task SetLockedByAsync(string lockedBy)
		{
			await UpdateFieldValueAsync(LockedByGuid, lockedBy).ConfigureAwait(false);
			LockedBy = lockedBy;
		}

		public double Progress { get; private set; }

		public async Task SetProgressAsync(double progress)
		{
			await UpdateFieldValueAsync(ProgressGuid, progress).ConfigureAwait(false);
			Progress = progress;
		}

		public BatchStatus Status { get; private set; }

		public async Task SetStatusAsync(BatchStatus status)
		{
			string statusDescription = status.GetDescription();
			await UpdateFieldValueAsync(StatusGuid, statusDescription).ConfigureAwait(false);
			Status = status;
		}

		public int TaggedItemsCount { get; private set; }

		public async Task SetTaggedItemsCountAsync(int taggedItemsCount)
		{
			await UpdateFieldValueAsync(TaggedItemsCountGuid, taggedItemsCount).ConfigureAwait(false);
			TaggedItemsCount = taggedItemsCount;
		}

		public async Task SetStartingIndexAsync(int newStartIndex)
		{
			await UpdateFieldValueAsync(StartingIndexGuid, newStartIndex).ConfigureAwait(false);

			StartingIndex = newStartIndex;
		}

		private async Task CreateAsync(int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			TotalItemsCount = totalItemsCount;
			StartingIndex = startingIndex;

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = syncConfigurationArtifactId
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Name = "Name"
							},
							Value = Guid.NewGuid()
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = TotalItemsCountGuid
							},
							Value = TotalItemsCount
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = StartingIndexGuid
							},
							Value = StartingIndex
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = StatusGuid
							},
							Value = BatchStatus.New.GetDescription()
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(_workspaceArtifactId, request).ConfigureAwait(false);

				ArtifactId = result.Object.ArtifactID;
			}
		}

		private async Task<bool> ReadLastAsync(int syncConfigurationArtifactId)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Fields = GetFieldsToRead(),
					Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}",
					IncludeNameInQueryResult = true,
					Sorts = new[]
					{
						new Sort
						{
							FieldIdentifier = new FieldRef
							{
								Guid = StartingIndexGuid
							},
							Direction = SortEnum.Descending
						}
					}
				};

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: 1).ConfigureAwait(false);

				if (result.TotalCount == 0)
				{
					return false;
				}

				ArtifactId = result.Objects[0].ArtifactID;
				PopulateBatchProperties(result.Objects[0]);
				return true;
			}
		}

		private async Task<bool> ReadNextAsync(int syncConfigurationArtifactId, int startingIndex)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Fields = GetFieldsToRead(),
					Condition = $"('{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}) AND ('{StartingIndexGuid}' > {startingIndex})",
					IncludeNameInQueryResult = true,
					Sorts = new[]
					{
						new Sort
						{
							FieldIdentifier = new FieldRef
							{
								Guid = StartingIndexGuid
							},
							Direction = SortEnum.Ascending
						}
					}
				};

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: 1).ConfigureAwait(false);

				if (result.TotalCount == 0)
				{
					return false;
				}

				ArtifactId = result.Objects[0].ArtifactID;
				PopulateBatchProperties(result.Objects[0]);
				return true;
			}
		}

		private async Task InitializeAsync(int artifactId)
		{
			ArtifactId = artifactId;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				// Do not use ReadAsync here. More details: REL-366692
				QueryRequest request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Guid = BatchObjectTypeGuid
					},
					Fields = GetFieldsToRead(),
					IncludeNameInQueryResult = true,
					Condition = $"'ArtifactID' == {artifactId}"
				};
				QueryResult queryResult = await objectManager.QueryAsync(_workspaceArtifactId, request, start: 0, length: 1).ConfigureAwait(false);
				if (!queryResult.Objects.Any())
				{
					throw new SyncException($"Batch ArtifactID: {artifactId} not found.");
				}
				PopulateBatchProperties(queryResult.Objects.Single());
			}
		}

		private async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int syncConfigurationArtifactId)
		{
			Task<IEnumerable<int>> getPausedBatches =
				ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Paused);

			Task<IEnumerable<int>> getNewBatches =
				ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.New);

			IEnumerable<int>[] allBatches = await Task.WhenAll(getPausedBatches, getNewBatches).ConfigureAwait(false);

			return allBatches.SelectMany(x => x);
		}

		private async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int syncConfigurationArtifactId)
		{
			Task<IEnumerable<IBatch>> getCompletedBatches = ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Completed);

			Task<IEnumerable<IBatch>> getCompletedWithErrorsBatches = ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.CompletedWithErrors);

			IEnumerable<IBatch>[] allBatches = await Task.WhenAll(getCompletedBatches, getCompletedWithErrorsBatches).ConfigureAwait(false);

			return allBatches.SelectMany(x => x);
		}

		private async Task<IEnumerable<int>> ReadBatchesIdsWithStatusAsync(int syncConfigurationArtifactId, BatchStatus batchStatus)
		{
			IEnumerable<int> batchIds = System.Array.Empty<int>();
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}'",
					IncludeNameInQueryResult = true
				};

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (result.TotalCount > 0)
				{
					batchIds = result.Objects.Select(x => x.ArtifactID);
				}
			}
			return batchIds;
		}

		private async Task<IEnumerable<IBatch>> ReadBatchesWithStatusAsync(int syncConfigurationArtifactId, BatchStatus batchStatus)
		{
			var batches = new ConcurrentBag<IBatch>();
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}'",
					IncludeNameInQueryResult = true
				};

				QueryResultSlim result = await objectManager.QuerySlimAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (result.TotalCount > 0)
				{
					IEnumerable<int> batchIds = result.Objects.Select(x => x.ArtifactID);

					Parallel.ForEach(batchIds, batchArtifactId =>
					{
						var batch = new Batch(_serviceFactory, _workspaceArtifactId);
						batch.InitializeAsync(batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
						batches.Add(batch);
					});
				}
			}
			return batches;
		}

		private async Task<IEnumerable<IBatch>> ReadAllAsync(int syncConfigurationArtifactId)
		{
			var batches = new ConcurrentBag<IBatch>();
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}",
					IncludeNameInQueryResult = true
				};

				QueryResultSlim result = await objectManager.QuerySlimAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (result.TotalCount > 0)
				{
					IEnumerable<int> batchIds = result.Objects.Select(x => x.ArtifactID);

					Parallel.ForEach(batchIds, batchArtifactId =>
					{
						var batch = new Batch(_serviceFactory, _workspaceArtifactId);
						batch.InitializeAsync(batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
						batches.Add(batch);
					});
				}
			}
			return batches;
		}

		private static FieldRef[] GetFieldsToRead()
		{
			return new[]
			{
				new FieldRef
				{
					Guid = TotalItemsCountGuid
				},
				new FieldRef
				{
					Guid = StartingIndexGuid
				},
				new FieldRef
				{
					Guid = StatusGuid
				},
				new FieldRef
				{
					Guid = FailedItemsCountGuid
				},
				new FieldRef
				{
					Guid = TransferredItemsCountGuid
				},
				new FieldRef
				{
					Guid = ProgressGuid
				},
				new FieldRef
				{
					Guid = LockedByGuid
				},
				new FieldRef
				{
					Guid = TaggedItemsCountGuid
				}
			};
		}

		private void PopulateBatchProperties(RelativityObject relativityObject)
		{
			TotalItemsCount = (int) relativityObject[TotalItemsCountGuid].Value;
			StartingIndex = (int) relativityObject[StartingIndexGuid].Value;
			Status = ((string)relativityObject[StatusGuid].Value).GetEnumFromDescription<BatchStatus>();
			FailedItemsCount = (int) (relativityObject[FailedItemsCountGuid].Value ?? default(int));
			TransferredItemsCount = (int) (relativityObject[TransferredItemsCountGuid].Value ?? default(int));
			TaggedItemsCount = (int) (relativityObject[TaggedItemsCountGuid].Value ?? default(int));
			MetadataBytesTransferred = Convert.ToInt64(relativityObject[MetadataBytesTransferredGuid].Value ?? default(long));
			FilesBytesTransferred = Convert.ToInt64(relativityObject[FilesBytesTransferredGuid].Value ?? default(long));
			TotalBytesTransferred = Convert.ToInt64(relativityObject[TotalBytesTransferredGuid].Value ?? default(long));
			Progress = decimal.ToDouble((decimal?) relativityObject[ProgressGuid].Value ?? default(decimal));
			LockedBy = (string) relativityObject[LockedByGuid].Value;
		}

		private async Task UpdateFieldValueAsync<T>(Guid fieldGuid, T value)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				UpdateRequest request = UpdateRequestExtensions.CreateForSingleField(ArtifactId, fieldGuid, value);
				await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public static async Task<IBatch> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId);
			await batch.CreateAsync(syncConfigurationArtifactId, totalItemsCount, startingIndex).ConfigureAwait(false);
			return batch;
		}

		public static async Task<IBatch> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId);
			await batch.InitializeAsync(artifactId).ConfigureAwait(false);
			return batch;
		}

		public static async Task<IEnumerable<IBatch>> GetAllAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId);
			IEnumerable<IBatch> batches = await batch.ReadAllAsync(syncConfigurationArtifactId).ConfigureAwait(false);
			return batches;
		}
	
		public static async Task<IBatch> GetLastAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId);
			bool batchFound = await batch.ReadLastAsync(syncConfigurationArtifactId).ConfigureAwait(false);
			return batchFound ? batch : null;
		}

		public static async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationId)
		{
			var batch = new Batch(serviceFactory, workspaceArtifactId);
			IEnumerable<int> batchIds = await batch.GetAllBatchesIdsToExecuteAsync(syncConfigurationId).ConfigureAwait(false);
			return batchIds;
		}

		public static async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationId)
		{
			var batch = new Batch(serviceFactory, workspaceArtifactId);
			IEnumerable<IBatch> batches = await batch.GetAllSuccessfullyExecutedBatchesAsync(syncConfigurationId).ConfigureAwait(false);
			return batches;
		}

		public static async Task<IBatch> GetNextAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId);
			bool batchFound = await batch.ReadNextAsync(syncConfigurationArtifactId, startingIndex).ConfigureAwait(false);
			return batchFound ? batch : null;
		}
	}
}