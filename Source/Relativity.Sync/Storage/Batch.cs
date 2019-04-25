using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

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
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");

		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");
		private static readonly Guid SyncConfigurationRelationGuid = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		private Batch(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public int ArtifactId { get; private set; }

		public int FailedItemsCount { get; private set; }

		public async Task SetFailedItemsCountAsync(int failedItemsCount)
		{
			await UpdateFieldValue(FailedItemsCountGuid, failedItemsCount).ConfigureAwait(false);
			FailedItemsCount = failedItemsCount;
		}

		public int TransferredItemsCount { get; private set; }

		public async Task SetTransferredItemsCountAsync(int transferredItemsCount)
		{
			await UpdateFieldValue(TransferredItemsCountGuid, transferredItemsCount).ConfigureAwait(false);
			TransferredItemsCount = transferredItemsCount;
		}

		public int TotalItemsCount { get; private set; }

		public int StartingIndex { get; private set; }

		public string LockedBy { get; private set; }

		public async Task SetLockedByAsync(string lockedBy)
		{
			await UpdateFieldValue(LockedByGuid, lockedBy).ConfigureAwait(false);
			LockedBy = lockedBy;
		}

		public double Progress { get; private set; }

		public async Task SetProgressAsync(double progress)
		{
			await UpdateFieldValue(ProgressGuid, progress).ConfigureAwait(false);
			Progress = progress;
		}

		public string Status { get; private set; }

		public async Task SetStatusAsync(string status)
		{
			await UpdateFieldValue(StatusGuid, status).ConfigureAwait(false);
			Status = status;
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

				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, 1, 1).ConfigureAwait(false);

				if (result.TotalCount == 0)
				{
					return false;
				}

				ArtifactId = result.Objects[0].ArtifactID;
				PopulateBatchProperties(result.Objects[0]);
				return true;
			}
		}

		private async Task ReadAsync(int workspaceArtifactId, int artifactId)
		{
			_workspaceArtifactId = workspaceArtifactId;
			ArtifactId = artifactId;
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				ReadRequest request = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = ArtifactId
					},
					Fields = GetFieldsToRead()
				};
				ReadResult readResult = await objectManager.ReadAsync(_workspaceArtifactId, request).ConfigureAwait(false);
				PopulateBatchProperties(readResult.Object);
			}
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
			Status = (string) relativityObject[StatusGuid].Value;
			FailedItemsCount = (int) (relativityObject[FailedItemsCountGuid].Value ?? default(int));
			TransferredItemsCount = (int) (relativityObject[TransferredItemsCountGuid].Value ?? default(int));
			Progress = decimal.ToDouble((decimal?) relativityObject[ProgressGuid].Value ?? default(decimal));
			LockedBy = (string) relativityObject[LockedByGuid].Value;
		}

		private async Task UpdateFieldValue<T>(Guid fieldGuid, T value)
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
			await batch.ReadAsync(workspaceArtifactId, artifactId).ConfigureAwait(false);
			return batch;
		}

		public static async Task<IBatch> GetLastAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			Batch batch = new Batch(serviceFactory);
			bool batchFound = await batch.ReadLastAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false);
			return batchFound ? batch : null;
		}
	}
}