using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class Batch : IBatch
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly int _syncConfigurationArtifactId;
		private readonly int _workspaceArtifactId;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");

		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");
		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");

		private Batch(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			_serviceFactory = serviceFactory;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;
			_workspaceArtifactId = workspaceArtifactId;
			TotalItemsCount = totalItemsCount;
			StartingIndex = startingIndex;
		}

		private Batch(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
			ArtifactId = artifactId;
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

		private async Task CreateAsync()
		{
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
						ArtifactID = _syncConfigurationArtifactId
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
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(_workspaceArtifactId, request).ConfigureAwait(false);

				ArtifactId = result.Object.ArtifactID;
			}
		}

		private async Task ReadAsync()
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				ReadRequest request = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = ArtifactId
					},
					Fields = new[]
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
					}
				};
				ReadResult readResult = await objectManager.ReadAsync(_workspaceArtifactId, request).ConfigureAwait(false);

				TotalItemsCount = (int) readResult.Object[TotalItemsCountGuid].Value;
				StartingIndex = (int) readResult.Object[StartingIndexGuid].Value;
				Status = readResult.Object[StatusGuid].Value.ToString();
				FailedItemsCount = (int) readResult.Object[FailedItemsCountGuid].Value;
				TransferredItemsCount = (int) readResult.Object[TransferredItemsCountGuid].Value;
				Progress = (double) readResult.Object[ProgressGuid].Value;
				LockedBy = readResult.Object[LockedByGuid].Value.ToString();
			}
		}

		private async Task UpdateFieldValue<T>(Guid fieldGuid, T value)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				UpdateRequest request = new UpdateRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = ArtifactId
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = fieldGuid
							},
							Value = value
						}
					}
				};
				await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public static async Task<IBatch> CreateAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			if (startingIndex > totalItemsCount)
			{
				throw new ArgumentException($"Starting index cannot be greater than total items count ({startingIndex} > {totalItemsCount}).");
			}

			Batch batch = new Batch(serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex);
			await batch.CreateAsync().ConfigureAwait(false);
			return batch;
		}

		public static async Task<IBatch> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int artifactId)
		{
			Batch batch = new Batch(serviceFactory, workspaceArtifactId, artifactId);
			await batch.ReadAsync().ConfigureAwait(false);
			return batch;
		}
	}
}