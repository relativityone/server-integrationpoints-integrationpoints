using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using kCura.WinEDDS.Service.Export;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class FieldManagerTests
	{
		private ConfigurationStub _configuration;
		private FieldManager _instance;
		private Mock<IFolderManager> _folderManager;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISearchManager> _searchManager;

		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _FOLDER_PATH_SOURCE_FIELD_NAME = "FolderPathSource";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private const string _FIELD_FOLDER_PATH_VALUE = "test3/test4";
		private const string _NATIVE_FILE_FILENAME_VALUE = "foo.txt";
		private const string _NATIVE_FILE_LOCATION_VALUE = "test1/test2";
		private const string _SOURCE_WORKSPACE_FOLDER_PATH_VALUE = "test5/test6/test7";
		private const long _NATIVE_FILE_SIZE_VALUE = 1013L;

		[SetUp]
		public void SetUp()
		{
			_configuration = new ConfigurationStub
			{
				// ISynchronizationConfiguration
				SourceWorkspaceArtifactId = 0,

				// IFieldConfiguration
				FolderPathSourceFieldName = _FOLDER_PATH_SOURCE_FIELD_NAME,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None
			};
			_configuration.SetFieldMappings(new List<FieldMap>());
			_configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles;

			_objectManager = new Mock<IObjectManager>();
			_searchManager = new Mock<ISearchManager>();
			_folderManager = new Mock<IFolderManager>();

			var searchManagerFactory = new Mock<ISearchManagerFactory>();
			searchManagerFactory.Setup(x => x.CreateSearchManagerAsync())
				.Returns(Task.FromResult(_searchManager.Object));

			var adminServiceFactory = new Mock<ISourceServiceFactoryForAdmin>();

			var userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
			userServiceFactory.Setup(x => x.CreateProxyAsync<IFolderManager>())
				.ReturnsAsync(_folderManager.Object);

			ContainerBuilder builder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReportingWithProgress(builder);
			builder.RegisterInstance(_configuration).AsImplementedInterfaces();
			builder.RegisterInstance(adminServiceFactory.Object).As<ISourceServiceFactoryForAdmin>();
			builder.RegisterInstance(userServiceFactory.Object).As<ISourceServiceFactoryForUser>();
			builder.RegisterInstance(searchManagerFactory.Object).As<ISearchManagerFactory>();

			// This is so we can resolve FieldManager directly. We would normally register it by its interface.
			builder.RegisterType<FieldManager>().As<FieldManager>();

			IContainer container = builder.Build();
			_instance = container.Resolve<FieldManager>();
		}

		private static IEnumerable<KeyValuePair<string, RelativityDataType>> MappedDocumentFieldTypePairs()
		{
			yield return new KeyValuePair<string, RelativityDataType>("Control Number", RelativityDataType.FixedLengthText);
			yield return new KeyValuePair<string, RelativityDataType>("Cool Field 1", RelativityDataType.Currency);
			yield return new KeyValuePair<string, RelativityDataType>("What's Up", RelativityDataType.Date);
			yield return new KeyValuePair<string, RelativityDataType>("Field: The Reckoning", RelativityDataType.YesNo);
			yield return new KeyValuePair<string, RelativityDataType>("Horrible \\ Field", RelativityDataType.WholeNumber);
		}

		private static IEnumerable<KeyValuePair<string, RelativityDataType>> SpecialDocumentFieldTypePairs()
		{
			yield return new KeyValuePair<string, RelativityDataType>("SupportedByViewer", RelativityDataType.FixedLengthText);
			yield return new KeyValuePair<string, RelativityDataType>("RelativityNativeType", RelativityDataType.FixedLengthText);
		}

		private static IEnumerable<FieldInfoDto> MappedDocumentFields() => MappedDocumentFieldTypePairs().Select(NameTypePairToDocumentFieldInfoDto);

		private static IEnumerable<FieldInfoDto> SpecialDocumentFields() => SpecialDocumentFieldTypePairs().Select(NameTypePairToDocumentFieldInfoDto);

		private static IEnumerable<FieldInfoDto> SpecialFields(DestinationFolderStructureBehavior folderStructureBehavior)
		{
			// These are not in any specific order.

			yield return FieldInfoDto.NativeFileLocationField();
			yield return FieldInfoDto.NativeFileFilenameField();
			yield return FieldInfoDto.NativeFileSizeField();

			foreach (FieldInfoDto field in SpecialDocumentFields())
			{
				yield return field;
			}

			if (folderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				yield return FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_SOURCE_FIELD_NAME);
			}
			else if (folderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				yield return FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure();
			}
		}

		[TestCase(DestinationFolderStructureBehavior.None)]
		[TestCase(DestinationFolderStructureBehavior.ReadFromField)]
		[TestCase(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)]
		public void ItShouldBuildAllSpecialFieldColumns(DestinationFolderStructureBehavior folderStructureBehavior)
		{
			// Arrange
			IEnumerable<FieldInfoDto> expectedSpecialFieldColumns = SpecialFields(folderStructureBehavior);
			_configuration.DestinationFolderStructureBehavior = folderStructureBehavior;

			// Act
			List<FieldInfoDto> specialFieldColumns = _instance.GetNativeSpecialFields().ToList();

			// Assert
			specialFieldColumns.Should().BeEquivalentTo(expectedSpecialFieldColumns);
		}

		[Test]
		public async Task ItShouldReturnAllDocumentFieldColumns()
		{
			// Arrange
			SetupDocumentFieldServices(MappedDocumentFieldTypePairs());

			// Act
			IEnumerable<FieldInfoDto> documentFields = await _instance
				.GetDocumentTypeFieldsAsync(CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			IEnumerable<FieldInfoDto> expectedDocumentFields = AssignIndicesToDocumentFields(MappedDocumentFields().Concat(SpecialDocumentFields()));
			documentFields.Should().BeEquivalentTo(expectedDocumentFields, opt => opt.WithStrictOrdering());
		}

		[TestCase(DestinationFolderStructureBehavior.None)]
		[TestCase(DestinationFolderStructureBehavior.ReadFromField)]
		[TestCase(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)]
		public async Task ItShouldReturnAllFieldColumns(DestinationFolderStructureBehavior folderStructureBehavior)
		{
			// Arrange
			IEnumerable<FieldInfoDto> expectedDocumentFields = MappedDocumentFields();
			IEnumerable<FieldInfoDto> expectedSpecialFields = SpecialFields(folderStructureBehavior);
			List<FieldInfoDto> expectedAllFields = AssignIndicesToDocumentFields(expectedDocumentFields.Concat(expectedSpecialFields)).ToList();

			_configuration.DestinationFolderStructureBehavior = folderStructureBehavior;
			SetupDocumentFieldServices(MappedDocumentFieldTypePairs());

			// Act
			IEnumerable<FieldInfoDto> allFields = await _instance
				.GetNativeAllFieldsAsync(CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			allFields.Should().BeEquivalentTo(expectedAllFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task CreateNativeSpecialFieldRowValueBuildersAsync_ShouldReturnSpecialFieldRowValueBuildersForNativeSpecialFieldType()
		{
			// Arrange
			SetupFileInfoFieldServices();

			// Act
			IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldRowValueBuilders = await _instance
				.CreateNativeSpecialFieldRowValueBuildersAsync(0, Array.Empty<int>())
				.ConfigureAwait(false);

			// Assert
			specialFieldRowValueBuilders.Should().NotBeNull();
			specialFieldRowValueBuilders.Keys.Should().BeEquivalentTo(new[] {
				SpecialFieldType.FolderPath,
				SpecialFieldType.NativeFileSize,
				SpecialFieldType.NativeFileLocation,
				SpecialFieldType.NativeFileFilename,
				SpecialFieldType.RelativityNativeType,
				SpecialFieldType.SupportedByViewer});
		}

		private static IEnumerable<TestCaseData> NativeFileInfoTestCases()
		{
			yield return new TestCaseData(FieldInfoDto.NativeFileFilenameField(), _NATIVE_FILE_FILENAME_VALUE)
			{
				TestName = "NativeFileFilename"
			};

			yield return new TestCaseData(FieldInfoDto.NativeFileLocationField(), _NATIVE_FILE_LOCATION_VALUE)
			{
				TestName = "NativeFileLocation"
			};

			yield return new TestCaseData(FieldInfoDto.NativeFileSizeField(), _NATIVE_FILE_SIZE_VALUE)
			{
				TestName = "NativeFileSize"
			};
		}

		[TestCaseSource(nameof(NativeFileInfoTestCases))]
		public async Task ItShouldBuildNativeFileInfo(FieldInfoDto field, object expectedValue)
		{
			// Arrange
			const int documentArtifactId = 1231;

			SetupDocumentFieldServices(MappedDocumentFieldTypePairs());
			SetupFileInfoFieldServices(new NativeFile(documentArtifactId, _NATIVE_FILE_LOCATION_VALUE, _NATIVE_FILE_FILENAME_VALUE, _NATIVE_FILE_SIZE_VALUE));

			// Act
			IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldRowValueBuilders =
				await _instance.CreateNativeSpecialFieldRowValueBuildersAsync(0, new int[] { documentArtifactId }).ConfigureAwait(false);

			RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = documentArtifactId };
			object value = specialFieldRowValueBuilders[field.SpecialFieldType].BuildRowValue(field, document, null);

			// Assert
			value.Should().Be(expectedValue);
		}

		private static IEnumerable<TestCaseData> FolderPathTestsCases()
		{
			yield return new TestCaseData(DestinationFolderStructureBehavior.ReadFromField, _FIELD_FOLDER_PATH_VALUE)
			{
				TestName = "ReadFromField"
			};
			yield return new TestCaseData(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure, _SOURCE_WORKSPACE_FOLDER_PATH_VALUE)
			{
				TestName = "RetainSourceWorkspaceStructure"
			};
		}

		[TestCaseSource(nameof(FolderPathTestsCases))]
		public async Task ItShouldBuildFolderPath(DestinationFolderStructureBehavior folderStructureBehavior, object expectedValue)
		{
			// Arrange
			const int documentArtifactId = 1231;
			const int folderArtifactId = 1023;

			_configuration.DestinationFolderStructureBehavior = folderStructureBehavior;

			SetupFileInfoFieldServices();
			SetupFolderPathFieldServices(
				_ => folderArtifactId,
				new FolderPath { ArtifactID = folderArtifactId, FullPath = _SOURCE_WORKSPACE_FOLDER_PATH_VALUE });

			// Act
			IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldRowValueBuilders =
				await _instance.CreateNativeSpecialFieldRowValueBuildersAsync(0, new int[] { documentArtifactId }).ConfigureAwait(false);

			FieldInfoDto fieldInfo = FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, _FOLDER_PATH_SOURCE_FIELD_NAME, _FOLDER_PATH_SOURCE_FIELD_NAME);
			RelativityObjectSlim document = new RelativityObjectSlim { ArtifactID = documentArtifactId };
			object value = specialFieldRowValueBuilders[fieldInfo.SpecialFieldType].BuildRowValue(fieldInfo, document, _FIELD_FOLDER_PATH_VALUE);

			// Assert
			value.Should().Be(expectedValue);
		}

		private void SetupDocumentFieldServices(IEnumerable<KeyValuePair<string, RelativityDataType>> mappedDocumentFields)
		{
			List<KeyValuePair<string, RelativityDataType>> mappedDocumentFieldList = mappedDocumentFields.ToList();
			List<KeyValuePair<string, RelativityDataType>> specialDocumentFieldLIst = SpecialDocumentFieldTypePairs().ToList();

			_configuration.SetFieldMappings(mappedDocumentFieldList.Select(NameTypePairToFieldMap).ToList());

			SetupQuerySlimAsync(q => q.ObjectType.Name == "Field").ReturnsQueryResultSlimAsync(q =>
			{
				List<RelativityObjectSlim> objects =
					FilterPairsUsingQueryCondition(q.Condition, mappedDocumentFieldList.Concat(specialDocumentFieldLIst))
						.Select(NameTypePairToRelativityObjectSlim)
						.ToList();
				return new QueryResultSlim { Objects = objects };
			});
		}

		private void SetupFileInfoFieldServices(params INativeFile[] nativeFileResponses)
		{
			DataSet result = GetDataSetForDocuments(nativeFileResponses);
			_searchManager.Setup(x => x.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>()))
				.Returns(result);
		}

		private static DataSet GetDataSetForDocuments(INativeFile[] nativeFiles)
		{
			DataSet dataSet = new DataSet();
			DataTable dataTable = new DataTable("Table1");
			dataSet.Tables.Add(dataTable);
			dataTable.Columns.AddRange(new[]
			{
				new DataColumn(_DOCUMENT_ARTIFACT_ID_COLUMN_NAME, typeof(int)),
				new DataColumn(_LOCATION_COLUMN_NAME, typeof(string)),
				new DataColumn(_FILENAME_COLUMN_NAME, typeof(string)),
				new DataColumn(_SIZE_COLUMN_NAME, typeof(long))
			});
			DataRow[] rows = nativeFiles.Select(nativeFile =>
			{
				DataRow dataRow = dataTable.NewRow();
				dataRow[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME] = nativeFile.DocumentArtifactId;
				dataRow[_LOCATION_COLUMN_NAME] = nativeFile.Location;
				dataRow[_FILENAME_COLUMN_NAME] = nativeFile.Filename;
				dataRow[_SIZE_COLUMN_NAME] = nativeFile.Size;
				return dataRow;
			}).ToArray();
			rows.ForEach(row => dataTable.Rows.Add(row));
			return dataSet;
		}

		private void SetupFolderPathFieldServices(Func<int, int> doc2Folder, params FolderPath[] folderPaths)
		{
			SetupQueryAsync(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Document).ReturnsQueryResultAsync(q =>
			{
				IEnumerable<int> requestedArtifactIds = ParseArtifactIdsFromQueryCondition(q.Condition);
				return new QueryResult
				{
					Objects = requestedArtifactIds.Select(x => new RelativityObject
					{
						ArtifactID = x,
						ParentObject = new RelativityObjectRef { ArtifactID = doc2Folder(x) }
					}).ToList()
				};
			});

			SetupFullPathListAsync()
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((_, folderIds) =>
					folderPaths.Where(x => folderIds.Contains(x.ArtifactID)).ToList());
		}

		private ISetup<IObjectManager, Task<QueryResultSlim>> SetupQuerySlimAsync(Func<QueryRequest, bool> requestMatcher)
		{
			return _objectManager.Setup(x =>
				x.QuerySlimAsync(It.IsAny<int>(),
					It.Is<QueryRequest>(q => requestMatcher(q)),
					It.IsAny<int>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>()));
		}

		private ISetup<IObjectManager, Task<QueryResult>> SetupQueryAsync(Func<QueryRequest, bool> requestMatcher)
		{
			return _objectManager.Setup(x =>
				x.QueryAsync(It.IsAny<int>(),
					It.Is<QueryRequest>(q => requestMatcher(q)),
					It.IsAny<int>(),
					It.IsAny<int>()));
		}

		private ISetup<IFolderManager, Task<List<FolderPath>>> SetupFullPathListAsync()
		{
			return _folderManager.Setup(x => x.GetFullPathListAsync(It.IsAny<int>(), It.IsAny<List<int>>()));
		}

		private static IEnumerable<int> ParseArtifactIdsFromQueryCondition(string condition)
		{
			global::System.Text.RegularExpressions.Match match = Regex.Match(condition, @" IN \[(.*)\]");
			if (match.Success)
			{
				return match.Groups[1]
					.Captures[0]
					.Value
					.Split(',')
					.Select(s => Convert.ToInt32(s, CultureInfo.InvariantCulture));
			}

			throw new ArgumentException($"Condition is not in correct format to parse ArtifactID list: \"{condition}\"");
		}

		private static IEnumerable<FieldInfoDto> AssignIndicesToDocumentFields(IEnumerable<FieldInfoDto> fields)
		{
			int index = 0;
			foreach (FieldInfoDto field in fields)
			{
				if (field.IsDocumentField)
				{
					field.DocumentFieldIndex = index;
					index += 1;
				}

				yield return field;
			}
		}

		private static IEnumerable<KeyValuePair<string, RelativityDataType>> FilterPairsUsingQueryCondition(
			string condition,
			IEnumerable<KeyValuePair<string, RelativityDataType>> fields)
		{
			foreach (KeyValuePair<string, RelativityDataType> pair in fields)
			{
				string quotedFieldName = $"'{KeplerQueryHelpers.EscapeForSingleQuotes(pair.Key)}'";
				if (condition.Contains(quotedFieldName))
				{
					yield return pair;
				}
			}
		}

		private static FieldMap NameTypePairToFieldMap(KeyValuePair<string, RelativityDataType> pair, int index)
		{
			return new FieldMap
			{
				SourceField = new FieldEntry { DisplayName = pair.Key, IsIdentifier = (index == 0) },
				DestinationField = new FieldEntry { DisplayName = pair.Key }
			};
		}

		private static RelativityObjectSlim NameTypePairToRelativityObjectSlim(KeyValuePair<string, RelativityDataType> pair)
		{
			return new RelativityObjectSlim
			{
				Values = new List<object>
				{
					pair.Key,
					pair.Value.GetDescription()
				}
			};
		}

		private static FieldInfoDto NameTypePairToDocumentFieldInfoDto(KeyValuePair<string, RelativityDataType> pair, int index)
		{
			FieldInfoDto fieldInfo;
			if (pair.Key == "SupportedByViewer")
			{
				fieldInfo = FieldInfoDto.SupportedByViewerField();
			}
			else if (pair.Key == "RelativityNativeType")
			{
				fieldInfo = FieldInfoDto.RelativityNativeTypeField();
			}
			else
			{
				fieldInfo = FieldInfoDto.DocumentField(pair.Key, pair.Key, index == 0);
			}
			fieldInfo.RelativityDataType = pair.Value;
			return fieldInfo;
		}
	}
}
