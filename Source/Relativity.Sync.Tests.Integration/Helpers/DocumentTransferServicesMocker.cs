using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Moq.Language;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	/// <summary>
	///     Mocks external interfaces necessary for testing document transfer (i.e. the source workspace data reader).
	/// </summary>
	internal sealed class DocumentTransferServicesMocker
	{
		private readonly MetadataMapping _metadataMapping;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");
		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");

		public Mock<ISourceServiceFactoryForUser> SourceServiceFactoryForUser { get; }
		public Mock<ISourceServiceFactoryForAdmin> SourceServiceFactoryForAdmin { get; }
		public Mock<IObjectManager> ObjectManager { get; }
		public Mock<IFileManager> FileManager { get; }

		public DocumentTransferServicesMocker(MetadataMapping metadataMapping)
		{
			_metadataMapping = metadataMapping;

			SourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			SourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			ObjectManager = new Mock<IObjectManager>();
			FileManager = new Mock<IFileManager>();
		}

		public void SetupServicesWithTestData(Document[] documents, int batchSize)
		{
			SetupServiceCreation(ObjectManager, SourceServiceFactoryForUser, SourceServiceFactoryForAdmin);
			SetupServiceCreation(FileManager, SourceServiceFactoryForUser, SourceServiceFactoryForAdmin);

			SetupBatches(batchSize, documents.Length);
			SetupExportResultBlocks(documents, batchSize);
			SetupNatives(documents);
		}

		public void RegisterMocks(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(SourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(SourceServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
		}

		private void SetupServiceCreation<T>(Mock<T> serviceMock,
			Mock<ISourceServiceFactoryForUser> userServiceFactory,
			Mock<ISourceServiceFactoryForAdmin> adminServiceFactory) where T : class, IDisposable
		{
			userServiceFactory.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
			adminServiceFactory.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
		}

		private void SetupBatches(int batchSize, int totalItemCount)
		{
			List<QueryResult> results = new List<QueryResult>();
			for (int i = 0; i < totalItemCount; i += batchSize)
			{
				int totalItemsInBatch = Math.Min(batchSize, totalItemCount - i);
				var result = new QueryResult
				{
					TotalCount = 1,
					Objects = new List<RelativityObject>
					{
						BatchObject(totalItemsInBatch, i, "New")
					}
				};
				results.Add(result);
			}

			SetupBatches(results);
		}

		private void SetupBatches(IEnumerable<QueryResult> queryResults)
		{
			ISetupSequentialResult<Task<QueryResult>> setupAssertion = ObjectManager.SetupSequence(x => x.QueryAsync(It.IsAny<int>(),
				It.Is<QueryRequest>(r => r.ObjectType.Guid == BatchObjectTypeGuid),
				It.IsAny<int>(),
				It.IsAny<int>()));
			foreach (QueryResult result in queryResults)
			{
				setupAssertion.ReturnsAsync(result);
			}
			setupAssertion.ReturnsAsync(new QueryResult());
		}

		private void SetupExportResultBlocks(Document[] documents, int batchSize)
		{
			for (int i = 0; i < documents.Length; i += batchSize)
			{
				int resultsBlockSize = Math.Min(batchSize, documents.Length - i);
				int exportIndexId = i;
				RelativityObjectSlim[] block = GetBlock(documents, resultsBlockSize, exportIndexId);

				SetupExportResultBlock(resultsBlockSize, exportIndexId, block);
			}
		}

		private RelativityObjectSlim[] GetBlock(Document[] documents, int resultsBlockSize, int startingIndex)
		{
			IEnumerable<FieldEntry> sourceDocumentFields = _metadataMapping.GetSourceDocumentFields();
			return documents.Skip(startingIndex)
				.Take(resultsBlockSize)
				.Select(x => ToRelativityObjectSlim(x, sourceDocumentFields)).ToArray();
		}

		private void SetupExportResultBlock(int resultsBlockSize, int exportIndexId, RelativityObjectSlim[] block)
		{
			ObjectManager.Setup(x => x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), resultsBlockSize, exportIndexId))
				.ReturnsAsync(block);
		}

		private void SetupNatives(Document[] documents)
		{
			FileManager
				.Setup(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync<int, int[], IFileManager, FileResponse[]>((_, ids) => DocumentsForArtifactIds(ids, documents));
		}

		private static RelativityObjectSlim ToRelativityObjectSlim(Document document, IEnumerable<FieldEntry> sourceDocumentFields)
		{
			Dictionary<string, object> fieldToValue = document.Values.ToDictionary(fv => fv.Field, fv => fv.Value);
			List<object> orderedValues = sourceDocumentFields.Select(x => fieldToValue[x.DisplayName]).ToList();

			return new RelativityObjectSlim
			{
				ArtifactID = document.ArtifactId,
				Values = orderedValues
			};
		}

		private static FileResponse[] DocumentsForArtifactIds(int[] requestedIds, Document[] documents)
		{
			Dictionary<int, Document> idToDocumentMap = documents.ToDictionary(d => d.ArtifactId);
			return requestedIds.Select(i => idToDocumentMap[i].ToFileResponse()).ToArray();
		}

		private static RelativityObject BatchObject(int totalItemCount, int startingIndex, string status)
		{
			return new RelativityObject
			{
				FieldValues = new List<FieldValuePair>
				{
					FieldValue(TotalItemsCountGuid, totalItemCount),
					FieldValue(StartingIndexGuid, startingIndex),
					FieldValue(StatusGuid, status),
					FieldValue(FailedItemsCountGuid, null),
					FieldValue(TransferredItemsCountGuid, null),
					FieldValue(ProgressGuid, null),
					FieldValue(LockedByGuid, null)
				}
			};
		}

		private static FieldValuePair FieldValue(Guid guid, object value)
		{
			return new FieldValuePair
			{
				Field = new Field { Guids = new List<Guid> { guid } },
				Value = value
			};
		}
	}
}
