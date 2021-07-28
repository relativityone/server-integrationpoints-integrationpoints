using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal abstract class SourceWorkspaceDataReaderTestsBase
	{
		protected IContainer _container;
		protected SourceWorkspaceDataReader _instance;
		protected DocumentTransferServicesMocker _documentTransferServicesMocker;
		protected ConfigurationStub _configuration;

		protected const string _DEFAULT_IDENTIFIER_COLUMN_NAME = "Control Number";
		private const int _USER_ARTIFACT_ID = 9;
		private const string _USER_EMAIL = "relativity.admin@kcura.com";
		private const int _WORKSPACE_ID = 12345;

		public void SetUp(int batchSize)
		{
			_configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _WORKSPACE_ID,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles
		};

			_documentTransferServicesMocker = new DocumentTransferServicesMocker();

			_container = ContainerHelper.CreateContainer(cb =>
			{
				IntegrationTestsContainerBuilder.MockReportingWithProgress(cb);
				_documentTransferServicesMocker.RegisterServiceMocks(cb);
				cb.RegisterInstance(_configuration).AsImplementedInterfaces();
			});

			Mock<IUserInfoManager> userInfoManagerMock = new Mock<IUserInfoManager>();
			userInfoManagerMock.Setup(m => m.RetrieveUsersBy(
				It.Is<int>(workspaceId => workspaceId == _WORKSPACE_ID),
				It.Is<QueryRequest>(query => query.Condition == $@"('ArtifactID' == {_USER_ARTIFACT_ID})"),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 1)
			)).ReturnsAsync(new UserInfoQueryResultSet()
			{
				ResultCount = 1,
				DataResults = new[] { new UserInfo { ArtifactID = _USER_ARTIFACT_ID, Email = _USER_EMAIL } }
			});
			_documentTransferServicesMocker.SourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IUserInfoManager>())
				.ReturnsAsync(userInfoManagerMock.Object);

			IFieldManager fieldManager = _container.Resolve<IFieldManager>();
			_documentTransferServicesMocker.SetFieldManager(fieldManager);

			_instance = CreateSourceWorkspaceDataReaderWithBatchSize(batchSize);
		}

		private SourceWorkspaceDataReader CreateSourceWorkspaceDataReaderWithBatchSize(int batchSize)
		{
			IRelativityExportBatcher batcher = CreateExporterForGivenBatchSize(batchSize);
			IBatchDataReaderBuilder batchDataReaderBuilder = CreateBatchDataReaderBuilder();
			IFieldManager fieldManager = _container.Resolve<IFieldManager>();
			ISyncLog syncLog = Mock.Of<ISyncLog>();

			return new SourceWorkspaceDataReader(
				batchDataReaderBuilder,
				_configuration,
				batcher,
				fieldManager,
				new ItemStatusMonitor(),
				syncLog,
				CancellationToken.None
			);
		}

		protected abstract IBatchDataReaderBuilder CreateBatchDataReaderBuilder();

		public void Dispose()
		{
			_instance?.Dispose();
			_container?.Dispose();
		}

		private IRelativityExportBatcher CreateExporterForGivenBatchSize(int batchSize)
		{
			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.TotalDocumentsCount).Returns(batchSize);
			IRelativityExportBatcher batcher = _container.Resolve<IRelativityExportBatcherFactory>().CreateRelativityExportBatcher(batch.Object);
			return batcher;
		}

		protected static DocumentImportJob CreateDefaultDocumentImportJob(int batchSize,
			Func<int, HashSet<FieldConfiguration>, Document> documentFactory, HashSet<FieldConfiguration> fields)
		{
			IList<FieldMap> fieldMappings = CreateFieldMappings(fields);
			Dictionary<string, RelativityDataType> schema = fields.ToDictionary(x => x.SourceColumnName, x => x.DataType);
			Document[] documents = Enumerable.Range(1, batchSize).Select(i => documentFactory(i, fields)).ToArray();

			return DocumentImportJob.Create(schema, fieldMappings, documents);
		}

		private static IList<FieldMap> CreateFieldMappings(HashSet<FieldConfiguration> additionalFields)
		{
			return additionalFields
				.Where(field => field.Type != FieldType.Special)
				.Select(field => field.Type == FieldType.Identifier
					? new FieldMap
					{
						FieldMapType = FieldMapType.Identifier,
						DestinationField = new FieldEntry { DisplayName = field.DestinationColumnName, IsIdentifier = true },
						SourceField = new FieldEntry { DisplayName = field.SourceColumnName, IsIdentifier = true }
					}
					: new FieldMap
					{
						FieldMapType = FieldMapType.None,
						DestinationField = new FieldEntry { DisplayName = field.DestinationColumnName },
						SourceField = new FieldEntry { DisplayName = field.SourceColumnName }
					}).ToList();
		}
		
		protected static Document CreateDocumentForNativesTransfer(int artifactId, HashSet<FieldConfiguration> values)
		{
			string nativeFileLocation = $"\\\\test\\foo\\foo{artifactId}.htm";
			string nativeFileFilename = $"foo{artifactId}.htm";
			const long initialSize = 100;
			long nativeFileSize = initialSize + artifactId;
			string workspaceFolderPath = string.Empty;
			string controlNumber = $"TST{artifactId.ToString("D4", CultureInfo.InvariantCulture)}";

			FieldValue[] fieldValues = values.Select(x => x.Type == FieldType.Identifier
				? new FieldValue(x.SourceColumnName, controlNumber)
				: new FieldValue(x.SourceColumnName, x.Value)).ToArray();

			return Document.Create(artifactId, nativeFileLocation, nativeFileFilename, nativeFileSize, workspaceFolderPath, fieldValues);
		}

		protected static HashSet<FieldConfiguration> IdentifierWithSpecialFields(string sourceColumnName, string destinationColumnName)
		{
			HashSet<FieldConfiguration> fields = new HashSet<FieldConfiguration>
			{
				FieldConfiguration.Identifier(sourceColumnName, destinationColumnName)
			};
			fields.UnionWith(DefaultSpecialFields);
			return fields;
		}

		protected static HashSet<FieldConfiguration> DefaultSpecialFields => new HashSet<FieldConfiguration>
		{
			FieldConfiguration.Special("RelativityNativeType", "RelativityNativeType", RelativityDataType.FixedLengthText, "Internet HTML"),
			FieldConfiguration.Special("SupportedByViewer", "SupportedByViewer", RelativityDataType.YesNo, true)
		};

		protected static HashSet<FieldConfiguration> DefaultIdentifierWithSpecialFields => IdentifierWithSpecialFields(_DEFAULT_IDENTIFIER_COLUMN_NAME, _DEFAULT_IDENTIFIER_COLUMN_NAME);

		protected static object RunThroughSerializer(object value)
		{
			var serializer = new JsonSerializer();
			var writer = new StringWriter();
			serializer.Serialize(writer, value);
			string serialized = writer.ToString();
			return serializer.Deserialize(new JsonTextReader(new StringReader(serialized)));
		}

	}
}