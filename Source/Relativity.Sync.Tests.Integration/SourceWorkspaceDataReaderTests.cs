using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderTests : IDisposable
	{
		private IContainer _container;
		private SourceWorkspaceDataReader _instance;
		private DocumentTransferServicesMocker _documentTransferServicesMocker;
		private ConfigurationStub _configuration;

		private const char _RECORD_SEPARATOR = (char)30;
		private const string _DEFAULT_IDENTIFIER_COLUMN_NAME = "Control Number";
		private const int _SINGLE_OBJECT_ARTIFACT_ID = 1;
		private const int _USER_ARTIFACT_ID = 9;
		private const string _USER_FULL_NAME = "Admin, Relativity";
		private const string _USER_EMAIL = "relativity.admin@kcura.com";
		private const int _WORKSPACE_ID = 12345;

		public void SetUp(int batchSize)
		{
			_configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _WORKSPACE_ID,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None
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
			IBatchDataReaderBuilder batchDataReaderBuilder = _container.Resolve<IBatchDataReaderBuilder>();
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

		public void Dispose()
		{
			_instance?.Dispose();
			_container?.Dispose();
		}

		private IRelativityExportBatcher CreateExporterForGivenBatchSize(int batchSize)
		{
			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.TotalItemsCount).Returns(batchSize);
			IRelativityExportBatcher batcher = _container.Resolve<IRelativityExportBatcherFactory>().CreateRelativityExportBatcher(batch.Object);
			return batcher;
		}

		private static IEnumerable<TestCaseData> ReturnCorrectValueForDifferentDataTypesTestCases()
		{
			yield return new TestCaseData(RelativityDataType.Currency, 1.0f)
				.Returns("1");

			DateTime expectedDateTime = new DateTime(2019, 6, 26);
			yield return new TestCaseData(RelativityDataType.Date, expectedDateTime)
				.Returns(expectedDateTime.ToString(CultureInfo.CurrentCulture));

			yield return new TestCaseData(RelativityDataType.Decimal, 2.0)
				.Returns("2");

			yield return new TestCaseData(RelativityDataType.File, "C:\\xd\\Import_03SmallNatives.zip")
				.Returns("C:\\xd\\Import_03SmallNatives.zip");

			yield return new TestCaseData(RelativityDataType.FixedLengthText, "Test1234")
				.Returns("Test1234");

			yield return new TestCaseData(RelativityDataType.LongText, "Test12345")
				.Returns("Test12345");

			yield return new TestCaseData(RelativityDataType.MultipleObject, GenerateMultipleObject("Test1", "Foo", "Bar Baz"))
				.Returns($"Test1{_RECORD_SEPARATOR}Foo{_RECORD_SEPARATOR}Bar Baz");

			yield return new TestCaseData(RelativityDataType.SingleChoice, GenerateSingleChoice("Cool Choice"))
				.Returns("Cool Choice");

			yield return new TestCaseData(RelativityDataType.SingleObject, GenerateSingleObject("Cool Object"))
				.Returns("Cool Object");

			yield return new TestCaseData(RelativityDataType.User, GenerateUser(_USER_FULL_NAME))
				.Returns(_USER_EMAIL);

			yield return new TestCaseData(RelativityDataType.WholeNumber, 15)
				.Returns("15");

			yield return new TestCaseData(RelativityDataType.YesNo, true)
				.Returns("True");
		}

		[TestCaseSource(nameof(ReturnCorrectValueForDifferentDataTypesTestCases))]
		public async Task<object> ItShouldReturnCorrectValueForDifferentBasicDataTypesAndMapFieldsCorrectly(RelativityDataType dataType, object initialValue)
		{
			// Arrange
			const int blockSize = 1;
			SetUp(blockSize);

			string sourceColumnName = dataType.ToString();
			string destinationColumnName = $"{sourceColumnName}_123";
			const string destinationIdentifierColumnName = "Alt Letter";

			HashSet<FieldConfiguration> fields = IdentifierWithSpecialFields(_DEFAULT_IDENTIFIER_COLUMN_NAME, destinationIdentifierColumnName);
			fields.Add(FieldConfiguration.Regular(sourceColumnName, destinationColumnName, dataType, initialValue));

			DocumentImportJob importData = CreateDefaultDocumentImportJob(blockSize, CreateDocumentForGenericSchema, fields);
			_configuration.SetFieldMappings(importData.FieldMappings);
			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, blockSize).ConfigureAwait(false);

			// Act
			_instance.Read();

			// Assert

			// Check identifier field
			Document document = importData.Documents.First();
			object expectedIdentifier = document.FieldValues.First(x => x.Field == _DEFAULT_IDENTIFIER_COLUMN_NAME).Value;
			_instance[destinationIdentifierColumnName].Should().Be(expectedIdentifier);

			// Check special fields
			foreach (FieldConfiguration specialField in DefaultSpecialFields)
			{
				object expectedValue = specialField.Value.ToString();
				_instance[specialField.DestinationColumnName].Should().Be(expectedValue);
			}

			// Check regular field
			return _instance[destinationColumnName];
		}

		[Test]
		public async Task ItShouldReturnLongTextStreamWhenGivenShibboleth()
		{
			// Arrange
			const int batchSize = 100;
			SetUp(batchSize);

			const string columnName = "LongText";
			const string bigTextShibboleth = "#KCURA99DF2F0FEB88420388879F1282A55760#";

			HashSet<FieldConfiguration> fields = DefaultIdentifierWithSpecialFields;
			fields.Add(FieldConfiguration.Regular(columnName, columnName, RelativityDataType.LongText, bigTextShibboleth));

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForGenericSchema, fields);
			_configuration.SetFieldMappings(importData.FieldMappings);

			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, batchSize).ConfigureAwait(false);

			Encoding encoding = Encoding.Unicode;
			const string expectedStreamContent = "Hello world!";
			_documentTransferServicesMocker.SetupLongTextStream(columnName, encoding, expectedStreamContent);

			// Act
			_instance.Read();
			int columnIndex = _instance.GetOrdinal(columnName);
			object actualValue = _instance.GetValue(columnIndex);

			// Assert
			var streamValue = actualValue as Stream;
			streamValue.Should().NotBeNull();

			var streamReader = new StreamReader(streamValue, encoding);
			string streamContents = streamReader.ReadToEnd();

			streamContents.Should().Be(expectedStreamContent);
		}

		[Test]
		public async Task ItShouldReturnCorrectMultipleChoiceTree()
		{
			// Arrange
			const int batchSize = 100;
			SetUp(batchSize);

			dynamic[] choiceValues =
			{
				new { Parent = 0, ArtifactID = 1, Name = "Foo" },
				new { Parent = 1, ArtifactID = 3, Name = "Bar" },
				new { Parent = 3, ArtifactID = 5, Name = "Baz" },
				new { Parent = 1, ArtifactID = 4, Name = "Bat" },
				new { Parent = 0, ArtifactID = 2, Name = "Bang" }
			};

			SetupObjectManagerForMultipleChoiceTree(choiceValues, _documentTransferServicesMocker.ObjectManager);

			List<dynamic> convertedForExport = choiceValues
				.Select(v => new { v.ArtifactID, v.Name })
				.Cast<dynamic>()
				.ToList();

			object inputValue = RunThroughSerializer(convertedForExport);
			const string columnName = "MultipleChoice";

			HashSet<FieldConfiguration> fields = DefaultIdentifierWithSpecialFields;
			fields.Add(FieldConfiguration.Regular(columnName, columnName, RelativityDataType.MultipleChoice, inputValue));

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForGenericSchema, fields);
			_configuration.SetFieldMappings(importData.FieldMappings);

			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, batchSize).ConfigureAwait(false);

			// Act
			_instance.Read();
			int columnIndex = _instance.GetOrdinal(columnName);
			object actualValue = _instance.GetValue(columnIndex);

			// Assert
			const char mult = (char)30;
			const char nest = (char)29;
			string expectedValue = $"Foo{nest}Bar{nest}Baz{mult}Foo{nest}Bat{mult}Bang{mult}";
			actualValue.Should().Be(expectedValue);
		}

		private void SetupObjectManagerForMultipleChoiceTree(dynamic[] values, Mock<IObjectManager> objectManager)
		{
			HashSet<int> registered = new HashSet<int>();
			foreach (dynamic value in values)
			{
				if (!registered.Contains(value.ArtifactID))
				{
					int artifactId = value.ArtifactID;
					int parentArtifactId = value.Parent;
					objectManager
						.Setup(x => x.ReadAsync(It.IsAny<int>(), It.Is<ReadRequest>(r => r.Object.ArtifactID == artifactId)))
						.ReturnsAsync(new ReadResult { Object = new RelativityObject { ParentObject = new RelativityObjectRef { ArtifactID = parentArtifactId } } });

					registered.Add(value.ArtifactID);
				}
			}
		}

		[Test]
		public async Task ItShouldReadMultipleBlocksAndConstructColumns()
		{
			// Arrange
			const int batchSize = 500;
			const int blockSize = 300;
			SetUp(batchSize);

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForGenericSchema, DefaultIdentifierWithSpecialFields);
			_configuration.SetFieldMappings(importData.FieldMappings);
			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, blockSize).ConfigureAwait(false);

			// Act/Assert
			foreach (Document document in importData.Documents)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);

				_instance["NativeFileFilename"].ConvertTo<string>().Should().Be(document.NativeFile.Filename);
				_instance["NativeFileLocation"].ConvertTo<string>().Should().Be(document.NativeFile.Location);
				_instance["NativeFileSize"].ConvertTo<long>().Should().Be(document.NativeFile.Size);

				foreach (FieldValue fieldValue in document.FieldValues)
				{
					Type expectedValueType = fieldValue.Value.GetType();
					_instance[fieldValue.Field].ConvertTo(expectedValueType).Should().Be(fieldValue.Value);
				}
			}

			bool hasExtraData = _instance.Read();

			_instance.ItemStatusMonitor.MarkReadSoFarAsSuccessful();
			foreach (Document document in importData.Documents)
			{
				_instance.ItemStatusMonitor.GetSuccessfulItemArtifactIds().Should().Contain(document.ArtifactId);
			}

			hasExtraData.Should().Be(false);
		}

		private static IEnumerable<TestCaseData> ApiCallsFailureSetups()
		{
			Tuple<Action<DocumentTransferServicesMocker>, string>[] failureActionAndNamePairs =
			{
				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCreation(), "Failing object manager creation"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingSearchManagerCreation(), "Failing file manager creation"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.Is<int>(x => x == 0))),
						"Failing first RetrieveResultsBlockFromExportAsync object manager call"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.Is<int>(x => x == 200), It.Is<int>(x => x == 300))),
						"Failing second RetrieveResultsBlockFromExportAsync object manager call"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingSearchManagerCall(fm =>
						fm.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>())),
						"Failing GetNativesForSearchAsync search manager call"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(r => r.ObjectType.Name == "Field"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())),
						"Failing QuerySlimAsync object manager call")
			};

			return failureActionAndNamePairs.Select(fa => new TestCaseData(fa.Item1) { TestName = fa.Item2 });
		}

		[TestCaseSource(nameof(ApiCallsFailureSetups))]
		public async Task ItShouldThrowSourceDataReaderExceptionWhenApiCallFails(Action<DocumentTransferServicesMocker> failureSetup)
		{
			// Arrange
			const int batchSize = 500;
			const int blockSize = 300;
			SetUp(batchSize);

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForGenericSchema, DefaultIdentifierWithSpecialFields);
			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, blockSize).ConfigureAwait(false);

			failureSetup(_documentTransferServicesMocker);

			// Act/Assert
			Assert.Throws<SourceDataReaderException>(() => importData.Documents.ForEach(x => _instance.Read()));
		}

		private static object GenerateSingleChoice(string name)
		{
			return RunThroughSerializer(new { Name = name });
		}

		private static object GenerateUser(string fullName)
		{
			return RunThroughSerializer(new { ArtifactID = _USER_ARTIFACT_ID, Name = fullName });
		}

		private static object GenerateSingleObject(string name)
		{
			return RunThroughSerializer(new { ArtifactID = _SINGLE_OBJECT_ARTIFACT_ID, Name = name });
		}

		private static object GenerateMultipleObject(params string[] names)
		{
			return RunThroughSerializer(names.Select(x => new { Name = x }).ToArray());
		}

		private static object RunThroughSerializer(object value)
		{
			var serializer = new JsonSerializer();
			var writer = new StringWriter();
			serializer.Serialize(writer, value);
			string serialized = writer.ToString();
			return serializer.Deserialize(new JsonTextReader(new StringReader(serialized)));
		}

		private static DocumentImportJob CreateDefaultDocumentImportJob(int batchSize,
			Func<int, HashSet<FieldConfiguration>, Document> documentFactory, HashSet<FieldConfiguration> fields)
		{
			IList<FieldMap> fieldMappings = CreateFieldMappings(fields);
			Dictionary<string, RelativityDataType> schema = fields.ToDictionary(x => x.SourceColumnName, x => x.DataType);
			Document[] documents = Enumerable.Range(1, batchSize).Select(i => documentFactory(i, fields)).ToArray();

			return DocumentImportJob.Create(schema, fieldMappings, documents);
		}

		private static HashSet<FieldConfiguration> DefaultIdentifierWithSpecialFields => IdentifierWithSpecialFields(_DEFAULT_IDENTIFIER_COLUMN_NAME, _DEFAULT_IDENTIFIER_COLUMN_NAME);

		private static HashSet<FieldConfiguration> IdentifierWithSpecialFields(string sourceColumnName, string destinationColumnName)
		{
			HashSet<FieldConfiguration> fields = new HashSet<FieldConfiguration>
			{
				FieldConfiguration.Identifier(sourceColumnName, destinationColumnName)
			};
			fields.UnionWith(DefaultSpecialFields);
			return fields;
		}

		private static HashSet<FieldConfiguration> DefaultSpecialFields => new HashSet<FieldConfiguration>
		{
			FieldConfiguration.Special("RelativityNativeType", "RelativityNativeType", RelativityDataType.FixedLengthText, "Internet HTML"),
			FieldConfiguration.Special("SupportedByViewer", "SupportedByViewer", RelativityDataType.YesNo, true)
		};

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

		private static Document CreateDocumentForGenericSchema(int artifactId, HashSet<FieldConfiguration> values)
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
	}
}
