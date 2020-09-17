using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class Batch : IBatch
	{
		private int _workspaceArtifactId;

		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");

		private static readonly Guid NameGuid = new Guid("3AB49704-F843-4E09-AFF2-5380B1BF7A35");
		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid TaggedItemsCountGuid = new Guid(); // TODO
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");

		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");
		private static readonly Guid SyncConfigurationRelationGuid = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");
		private static readonly Guid ConfigurationObjectTypeGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		private Batch(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
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

		private async Task CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			_workspaceArtifactId = workspaceArtifactId;
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
								Guid = NameGuid
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

		private async Task<bool> ReadLastAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Fields = GetFieldsToRead(),
					Condition = $"'{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationArtifactId}",
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

		private async Task<bool> ReadNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			_workspaceArtifactId = workspaceArtifactId;

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Fields = GetFieldsToRead(),
					Condition = $"('{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationArtifactId}) AND ('{StartingIndexGuid}' > {startingIndex})",
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

		private async Task InitializeAsync(int workspaceArtifactId, int artifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;
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
					Condition = $"'ArtifactID' == {artifactId}"
				};
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, start: 0, length: 1).ConfigureAwait(false);
				if (!queryResult.Objects.Any())
				{
					throw new SyncException($"Batch ArtifactID: {artifactId} not found.");
				}
				PopulateBatchProperties(queryResult.Objects.Single());
			}
		}

		private async Task<IEnumerable<int>> ReadAllNewIdsAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;

			IEnumerable<int> batchIds = System.Array.Empty<int>();
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{BatchStatus.New.GetDescription()}'"
				};

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (result.TotalCount > 0)
				{
					batchIds = result.Objects.Select(x => x.ArtifactID);
				}
			}
			return batchIds;
		}

		private async Task<IEnumerable<IBatch>> ReadAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;

			var batches = new ConcurrentBag<IBatch>();
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationArtifactId}"
				};

				QueryResultSlim result = await objectManager.QuerySlimAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (result.TotalCount > 0)
				{
					IEnumerable<int> batchIds = result.Objects.Select(x => x.ArtifactID);

					Parallel.ForEach(batchIds, batchArtifactId =>
					{
						var batch = new Batch(_serviceFactory);
						batch.InitializeAsync(workspaceArtifactId, batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
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
			Batch batch = new Batch(serviceFactory);
			await batch.CreateAsync(workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex).ConfigureAwait(false);
			return batch;
		}

		public static async Task<IBatch> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			Batch batch = new Batch(serviceFactory);
			await batch.InitializeAsync(workspaceArtifactId, artifactId).ConfigureAwait(false);
			return batch;
		}

		public static async Task<IEnumerable<IBatch>> GetAllAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			Batch batch = new Batch(serviceFactory);
			IEnumerable<IBatch> batches = await batch.ReadAllAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false);
			return batches;
		}
		
		public static async Task DeleteAllForConfigurationAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new MassDeleteByCriteriaRequest
				{
					ObjectIdentificationCriteria = new ObjectIdentificationCriteria()
					{
						ObjectType = new ObjectTypeRef
						{
							Guid = BatchObjectTypeGuid
						},
						Condition = $"'{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationArtifactId}"
					}
				};
				await objectManager.DeleteAsync(workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public static async Task DeleteAllOlderThanAsync(ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime, int workspaceArtifactId, TimeSpan olderThan)
		{
			IEnumerable<int> oldConfiguratiosArtifactIds = await GetConfigurationsOlderThanAsync(serviceFactory, dateTime, workspaceArtifactId, olderThan).ConfigureAwait(false);
			IEnumerable<Task> deleteTasks = oldConfiguratiosArtifactIds.Select(configurationArtifactId => DeleteAllForConfigurationAsync(serviceFactory, workspaceArtifactId, configurationArtifactId));
			await Task.WhenAll(deleteTasks).ConfigureAwait(false);
		}

		private static async Task<IEnumerable<int>> GetConfigurationsOlderThanAsync(ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime, int workspaceArtifactId, TimeSpan olderThan)
		{
			DateTime createdBeforeDate = dateTime.UtcNow - olderThan;
			DateTime GetObjectCreationDate(RelativityObject relativityObject) => (DateTime)relativityObject.FieldValues.Single().Value;

			using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Guid = ConfigurationObjectTypeGuid
					},
					Fields = new[]
					{
						new FieldRef()
						{
							Name = "System Created On"
						}
					}
				};

				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, 0, int.MaxValue).ConfigureAwait(false);
				IEnumerable<RelativityObject> oldConfigurations = queryResult.Objects.Where(configurationObject =>
					GetObjectCreationDate(configurationObject) < createdBeforeDate);
				return oldConfigurations.Select(x => x.ArtifactID);
			}
		}

		public static async Task<IBatch> GetLastAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			Batch batch = new Batch(serviceFactory);
			bool batchFound = await batch.ReadLastAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false);
			return batchFound ? batch : null;
		}

		public static async Task<IEnumerable<int>> GetAllNewBatchIdsAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationId)
		{
			var batch = new Batch(serviceFactory);
			IEnumerable<int> batchIds = await batch.ReadAllNewIdsAsync(workspaceArtifactId, syncConfigurationId).ConfigureAwait(false);
			return batchIds;
		}

		public static async Task<IBatch> GetNextAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			Batch batch = new Batch(serviceFactory);
			bool batchFound = await batch.ReadNextAsync(workspaceArtifactId, syncConfigurationArtifactId, startingIndex).ConfigureAwait(false);
			return batchFound ? batch : null;
		}
	}
}