using System;
using System.Collections.Generic;
using System.Data;
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
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Transport;
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
		private Transfer.IFieldManager _fieldManager;

		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _IDENTIFIER_COLUMN_NAME = "Identifier";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _SIZE_COLUMN_NAME = "Size";

		public Mock<ISourceServiceFactoryForUser> ServiceFactoryForUser { get; }
		public Mock<ISourceServiceFactoryForAdmin> ServiceFactoryForAdmin { get; }
		public Mock<IObjectManager> ObjectManager { get; }
		public Mock<ISearchService> SearchService { get; }

        public DocumentTransferServicesMocker()
		{
			ServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			ServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			ObjectManager = new Mock<IObjectManager>();
			SearchService = new Mock<ISearchService>();
		}

		private void SetupServicesWithTestData(DocumentImportJob job)
		{
			SetupServiceCreation(ObjectManager);
            SetupServiceCreation(SearchService);
			SetupFields(job.Schema);
		}

		public async Task SetupServicesWithNativesTestDataAsync(DocumentImportJob job, int batchSize)
		{
			SetupServicesWithTestData(job);
			await SetupNativesExportResultBlocksAsync(_fieldManager, job.Documents, batchSize).ConfigureAwait(false);
			SetupNatives(job.Documents);

			// We should also setup folder paths here. That should be done once we are able to reliably
			// mock out workflows other than those using DestinationFolderPathBehavior.None.
		}

		public void SetupServicesWithImagesTestDataAsync(DocumentImportJob job, int batchSize)
		{
			SetupServicesWithTestData(job);
			SetupImagesExportResultBlocks(job.Documents, batchSize);
			SetupImages(job.Documents);
		}

		public void SetupFailingObjectManagerCreation()
		{
			SetupFailingServiceCreation<IObjectManager>();
		}

		public void SetupFailingSearchServiceCreation()
		{
			SetupFailingServiceCreation<ISearchService>();
		}
		
        public void SetupFailingObjectManagerCall<TResult>(Expression<Func<IObjectManager, TResult>> expression)
		{
			ObjectManager.Setup(expression).Throws<AggregateException>();
		}

		public void SetupFailingSearchServiceCall<TResult>(Expression<Func<ISearchService, TResult>> expression)
		{
			SearchService.Setup(expression).Throws<AggregateException>();
		}

		public void RegisterServiceMocks(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(ServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(ServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
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

		public void SetFieldManager(Relativity.Sync.Transfer.IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
		}

		private void SetupServiceCreation<T>(Mock<T> serviceMock) where T : class, IDisposable
		{
			ServiceFactoryForUser.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
			ServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<T>()).ReturnsAsync(serviceMock.Object);
		}

		private void SetupFailingServiceCreation<T>() where T : class, IDisposable
		{
			ServiceFactoryForUser.Setup(x => x.CreateProxyAsync<T>()).ThrowsAsync(new AggregateException());
			ServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<T>()).ThrowsAsync(new AggregateException());
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
			global::System.Text.RegularExpressions.Match match = Regex.Match(condition, @"^'Name' IN \[([^]]+)\]", RegexOptions.IgnoreCase);
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
		
		private void SetupExportResultBlocks(Document[] documents, int batchSize, IList<FieldInfoDto> sourceDocumentFields)
		{
			for (int takenDocumentsCount = 0; takenDocumentsCount < documents.Length; takenDocumentsCount += batchSize)
			{
				int remainingDocumentCount = documents.Length - takenDocumentsCount;
				int resultsBlockSize = Math.Min(batchSize, remainingDocumentCount);
				int exportIndexId = takenDocumentsCount;
				RelativityObjectSlim[] block = GetBlock(sourceDocumentFields, documents, resultsBlockSize, exportIndexId);

				SetupExportResultBlock(remainingDocumentCount, exportIndexId, block);
			}
		}

		private void SetupImagesExportResultBlocks(Document[] documents, int batchSize)
		{
			SetupExportResultBlocks(documents, batchSize, new List<FieldInfoDto>()
			{
				FieldInfoDto.DocumentField("Control Number", "Control Number", true)
			});
		}

		private async Task SetupNativesExportResultBlocksAsync(Transfer.IFieldManager fieldManager, Document[] documents,
			int batchSize)
		{
			IList<FieldInfoDto> sourceDocumentFields = await fieldManager.GetDocumentTypeFieldsAsync(CancellationToken.None).ConfigureAwait(false);
			SetupExportResultBlocks(documents, batchSize, sourceDocumentFields);
		}

		private void SetupExportResultBlock(int resultsBlockSize, int exportIndexId, RelativityObjectSlim[] block)
		{
			ObjectManager.Setup(x => x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), resultsBlockSize, exportIndexId))
				.ReturnsAsync(block);
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

		private void SetupNatives(Document[] documents)
		{
            DataSetWrapper searchServiceDataSet = GetSearchServiceDataSetForDocumentsWithNatives(documents);
            SearchService
                .Setup(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(searchServiceDataSet);
		}

        private static DataSetWrapper GetSearchServiceDataSetForDocumentsWithNatives(Document[] documents)
        {
			DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable("DataTableWithNatives");
            dataSet.Tables.Add(dataTable);
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(_DOCUMENT_ARTIFACT_ID_COLUMN_NAME, typeof(int)),
                new DataColumn(_LOCATION_COLUMN_NAME, typeof(string)),
                new DataColumn(_FILENAME_COLUMN_NAME, typeof(string)),
                new DataColumn(_SIZE_COLUMN_NAME, typeof(long))
            });
            DataRow[] rows = documents.Select(document =>
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME] = document.ArtifactId;
                dataRow[_LOCATION_COLUMN_NAME] = document.NativeFile.Location;
                dataRow[_FILENAME_COLUMN_NAME] = document.NativeFile.Filename;
                dataRow[_SIZE_COLUMN_NAME] = document.NativeFile.Size;
                return dataRow;
            }).ToArray();
            rows.ForEach(row => dataTable.Rows.Add(row));

			return new DataSetWrapper(dataSet);
        }
		
		private void SetupImages(Document[] documents)
		{
            DataSetWrapper searchServiceDataSet = GetSearchServiceDataSetForDocumentsWithImages(documents);
            SearchService
                .Setup(x => x.RetrieveImagesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>(), It.IsAny<string>()))
                .ReturnsAsync(searchServiceDataSet);
		}
		
        private static DataSetWrapper GetSearchServiceDataSetForDocumentsWithImages(Document[] documents)
        {
			DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable("DataTableWithImages");
            dataSet.Tables.Add(dataTable);
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(_DOCUMENT_ARTIFACT_ID_COLUMN_NAME, typeof(int)),
                new DataColumn(_FILENAME_COLUMN_NAME, typeof(string)),
                new DataColumn(_IDENTIFIER_COLUMN_NAME, typeof(string)),
                new DataColumn(_LOCATION_COLUMN_NAME, typeof(string)),
                new DataColumn(_SIZE_COLUMN_NAME, typeof(long))
            });

            foreach (Document document in documents)
            {
                foreach (ImageFile image in document.Images)
                {
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME] = document.ArtifactId;
                    dataRow[_FILENAME_COLUMN_NAME] = image.Filename;
                    dataRow[_IDENTIFIER_COLUMN_NAME] = image.Identifier;
                    dataRow[_LOCATION_COLUMN_NAME] = image.Location;
                    dataRow[_SIZE_COLUMN_NAME] = image.Size;
                    dataTable.Rows.Add(dataRow);
                }
            }
			
			return new DataSetWrapper(dataSet);
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