using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	/// <summary>
	///     Mocks external interfaces necessary for testing document transfer (i.e. the source workspace data reader).
	/// </summary>
	internal sealed class DocumentTransferServicesMocker
	{
		private IFieldManager _fieldManager;

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

		public DocumentTransferServicesMocker()
		{
			SourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			SourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			ObjectManager = new Mock<IObjectManager>();
			FileManager = new Mock<IFileManager>();
		}

		public async Task SetupServicesWithTestData(DocumentImportJob job, int batchSize)
		{
			SetupServiceCreation(ObjectManager, SourceServiceFactoryForUser, SourceServiceFactoryForAdmin);
			SetupServiceCreation(FileManager, SourceServiceFactoryForUser, SourceServiceFactoryForAdmin);

			SetupFields(job.Schema);
			SetupBatches(batchSize, job.Documents.Length);
			await SetupExportResultBlocks(_fieldManager, job.Documents, batchSize).ConfigureAwait(false);
			SetupNatives(job.Documents);
			//SetupFolderPaths(job.Documents);
		}

		public void RegisterServiceMocks(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(SourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(SourceServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
		}

		public void SetFieldManager(IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
		}

		private void SetupServiceCreation<T>(Mock<T> serviceMock,
			Mock<ISourceServiceFactoryForUser> userServiceFactory,
			Mock<ISourceServiceFactoryForAdmin> adminServiceFactory) where T : class, IDisposable
		{
			userServiceFactory.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
			adminServiceFactory.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
		}

		private void SetupFields(IReadOnlyDictionary<string, RelativityDataType> fieldSchema)
		{
			SetupQuerySlimForFields()
				.ReturnsAsync<int, QueryRequest, int, int, CancellationToken, IObjectManager, QueryResultSlim>((ws, req, s, l, c) => SelectFieldResults(req.Condition, fieldSchema));
		}

		private ISetup<IObjectManager, Task<QueryResultSlim>> SetupQuerySlimForFields()
		{
			return ObjectManager.Setup(x =>
				x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(r => r.ObjectType.Name == "Field"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()));
		}

		// NOTE: Successful operation of this method depends on implementation details in DocumentFieldRepository.
		// NOTE: If the condition or result parsing logic changes, this method will need to be updated.
		private static QueryResultSlim SelectFieldResults(string condition, IReadOnlyDictionary<string, RelativityDataType> fieldNameToDataType)
		{
			System.Text.RegularExpressions.Match match = Regex.Match(condition, @"^'Name' IN \[([^]]+)\]", RegexOptions.IgnoreCase);
			if (match == null)
			{
				throw new ArgumentException($"Could not find field name pattern in field name query's condition: {condition}", nameof(condition));
			}

			string fieldNamesArrayRaw = match.Groups[1].Captures[0].Value;
			IEnumerable<string> fieldNames = fieldNamesArrayRaw
				.Split(new[] { ", " }, StringSplitOptions.None)
				.Select(x => x.Trim('"').Trim('\''));

			List<RelativityObjectSlim> objects = fieldNames
				.Select(f => new RelativityObjectSlim { Values = new List<object> { f, fieldNameToDataType[f].ToRelativityTypeDisplayName()} })
				.ToList();

			QueryResultSlim retVal = new QueryResultSlim { Objects = objects };
			return retVal;
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

		private async Task SetupExportResultBlocks(IFieldManager fieldManager, Document[] documents, int batchSize)
		{
			IList<Transfer.FieldInfoDto> sourceDocumentFields = await fieldManager.GetDocumentFieldsAsync(CancellationToken.None).ConfigureAwait(false);
			for (int i = 0; i < documents.Length; i += batchSize)
			{
				int resultsBlockSize = Math.Min(batchSize, documents.Length - i);
				int exportIndexId = i;
				RelativityObjectSlim[] block = GetBlock(sourceDocumentFields, documents, resultsBlockSize, exportIndexId);

				SetupExportResultBlock(resultsBlockSize, exportIndexId, block);
			}
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

		// TODO: Make this folder path stuff for something other than DestinationFolderPathBehavior.None.

		//private void SetupFolderPaths(Document[] documents)
		//{
		//	ObjectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(req => req.ObjectType.ArtifactTypeID == 10), It.IsAny<int>(), It.IsAny<int>()))
		//		.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((ws, req, s, l) => SelectFolderPathResults(req.Condition, documents));
		//}

		//private static bool Select

		//private static QueryResult SelectFolderPathResults(string condition, Document[] documents)
		//{

		//}

		private static RelativityObjectSlim[] GetBlock(IList<Transfer.FieldInfoDto> sourceDocumentFields, Document[] documents, int resultsBlockSize, int startingIndex)
		{
			return documents.Skip(startingIndex)
				.Take(resultsBlockSize)
				.Select(x => ToRelativityObjectSlim(x, sourceDocumentFields)).ToArray();
		}

		private static RelativityObjectSlim ToRelativityObjectSlim(Document document, IEnumerable<Transfer.FieldInfoDto> sourceDocumentFields)
		{
			Dictionary<string, object> fieldToValue = document.FieldValues.ToDictionary(fv => fv.Field, fv => fv.Value);
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
