using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Moq.Language.Flow;
using Relativity.Kepler.Transport;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration.Helpers
{
	/// <summary>
	///     Mocks external interfaces necessary for testing document transfer (i.e. the source workspace data reader).
	/// </summary>
	internal sealed class DocumentTransferServicesMocker
	{
		private IFieldManager _fieldManager;

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
			SetupServiceCreation(ObjectManager);
			SetupServiceCreation(FileManager);

			SetupFields(job.Schema);
			await SetupExportResultBlocks(_fieldManager, job.Documents, batchSize).ConfigureAwait(false);
			SetupNatives(job.Documents);

			// We should also setup folder paths here. That should be done once we are able to reliably
			// mock out workflows other than those using DestinationFolderPathBehavior.None.
		}

		public void SetupFailingObjectManagerCreation()
		{
			SetupFailingServiceCreation<IObjectManager>();
		}

		public void SetupFailingFileManagerCreation()
		{
			SetupFailingServiceCreation<IFileManager>();
		}

		public void SetupFailingObjectManagerCall<TResult>(Expression<Func<IObjectManager, TResult>> expression)
		{
			ObjectManager.Setup(expression).Throws<AggregateException>();
		}

		public void SetupFailingFileManagerCall<TResult>(Expression<Func<IFileManager, TResult>> expression)
		{
			FileManager.Setup(expression).Throws<AggregateException>();
		}

		public void RegisterServiceMocks(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(SourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(SourceServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
		}

		public void SetupLongTextStream(string fieldName, Encoding encoding, string streamContents)
		{
			var keplerStream = new Mock<IKeplerStream>();

			int docArtifactId = (fieldName + streamContents).GetHashCode();
			ObjectManager
				.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(q => MatchesQueryByIdentifierRequest(q)), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(QueryResultSlimForArtifactIDs(docArtifactId));

			bool isUnicode = encoding.Equals(Encoding.Unicode);
			ObjectManager
				.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(q => MatchesFieldUnicodeQueryRequest(q)), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(QueryResultSlimForValues(new List<object> { isUnicode }));

			ObjectManager
				.Setup(x => x.StreamLongTextAsync(It.IsAny<int>(), It.Is<RelativityObjectRef>(y => y.ArtifactID == docArtifactId), It.Is<FieldRef>(y => y.Name == "LongText")))
				.ReturnsAsync(keplerStream.Object);

			keplerStream.Setup(x => x.GetStreamAsync())
				.ReturnsAsync(new MemoryStream(encoding.GetBytes(streamContents)));
		}

		public void SetFieldManager(IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
		}

		private void SetupServiceCreation<T>(Mock<T> serviceMock) where T : class, IDisposable
		{
			SourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
			SourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
		}

		private void SetupFailingServiceCreation<T>() where T : class, IDisposable
		{
			SourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<T>()).ThrowsAsync(new AggregateException());
			SourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<T>()).ThrowsAsync(new AggregateException());
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
				.Select(f => new RelativityObjectSlim { Values = new List<object> { f, fieldNameToDataType[f].GetDescription() } })
				.ToList();

			QueryResultSlim retVal = new QueryResultSlim { Objects = objects };
			return retVal;
		}
		
		private async Task SetupExportResultBlocks(IFieldManager fieldManager, Document[] documents, int batchSize)
		{
			IList<FieldInfoDto> sourceDocumentFields = await fieldManager.GetDocumentFieldsAsync(CancellationToken.None).ConfigureAwait(false);
			for (int takenDocumentsCount = 0; takenDocumentsCount < documents.Length; takenDocumentsCount += batchSize)
			{
				int remainingDocumentCount = documents.Length - takenDocumentsCount;
				int resultsBlockSize = Math.Min(batchSize, remainingDocumentCount);
				int exportIndexId = takenDocumentsCount;
				RelativityObjectSlim[] block = GetBlock(sourceDocumentFields, documents, resultsBlockSize, exportIndexId);

				SetupExportResultBlock(remainingDocumentCount, exportIndexId, block);
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

		private static RelativityObjectSlim[] GetBlock(IList<FieldInfoDto> sourceDocumentFields, Document[] documents, int resultsBlockSize, int startingIndex)
		{
			return documents.Skip(startingIndex)
				.Take(resultsBlockSize)
				.Select(x => ToRelativityObjectSlim(x, sourceDocumentFields)).ToArray();
		}

		private static RelativityObjectSlim ToRelativityObjectSlim(Document document, IEnumerable<FieldInfoDto> sourceDocumentFields)
		{
			Dictionary<string, object> fieldToValue = document.FieldValues.ToDictionary(fv => fv.Field, fv => fv.Value);
			List<object> orderedValues = sourceDocumentFields.Select(x => fieldToValue[x.SourceFieldName]).ToList();

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

		private static bool MatchesQueryByIdentifierRequest(QueryRequest request)
		{
			return request.ObjectType.ArtifactTypeID == (int)ArtifactType.Document && request.Condition.Contains("'Control Number' ==");
		}

		private static bool MatchesFieldUnicodeQueryRequest(QueryRequest request)
		{
			return request.ObjectType.Name == "Field" && request.Fields.First().Name == "Unicode";
		}

		private static QueryResultSlim QueryResultSlimForArtifactIDs(params int[] artifactIds)
		{
			return new QueryResultSlim
			{
				Objects = artifactIds.Select(x => new RelativityObjectSlim { ArtifactID = x }).ToList()
			};
		}

		private static QueryResultSlim QueryResultSlimForValues(params List<object>[] valueSets)
		{
			return new QueryResultSlim
			{
				Objects = valueSets.Select(x => new RelativityObjectSlim { Values = x }).ToList()
			};
		}
	}
}
