using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using kCura.IntegrationPoints.Data.Tests.Facades.Implementations.TestCases;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations
{
	[TestFixture]
	public class ObjectManagerFacadeDiscoverHeavyRequestDecoratorTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<IObjectManagerFacade> _objectManagerMock;
		private ObjectManagerFacadeDiscoverHeavyRequestDecorator _sut;

		private static IEnumerable<FieldValueTestCases> FieldValueExceededAndNotTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
							.ToArray(),
						name: "Array Field Exceeded"
					),
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
							.ToArray(),
						name: "Array Field Not Exceeded"
					)
				}
			},
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
							.ToList(),
						name: "List Field Exceeded"
					),
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
							.ToArray(),
						name: "List Field Not Exceeded"
					)
				}
			}
		};

		private static IEnumerable<FieldValueTestCases> FieldValueExceededByOneTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
							.ToArray(),
						name: "Array Field"
					)
				}
			},
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
							.ToList(),
						name: "List Field"
					)
				}
			}
		};

		private static IEnumerable<FieldValueTestCases> FieldValueEqualToMaxTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
							.ToArray(),
						name: "Array Field"
					)
				}
			},
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
							.ToList(),
						name: "List Field"
					)
				}
			}
		};

		private static IEnumerable<FieldValueTestCases> FieldValueLowerThanMaxTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT - 1)
							.ToArray(),
						name: "Array Field"
					)
				}
			},
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(1, _MAX_COLLECTION_COUNT - 1)
							.ToList(),
						name: "List Field"
					)
				}
			}
		};

		private static IEnumerable<FieldValueTestCases> FieldValueAreEmptyTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: Enumerable.Range(0, 0).ToArray(),
						name: "Array Field"
					)
				}
			},
			new FieldValueTestCases {
				FieldValues = new [] {
					new FieldValueTestCase(
						value: Enumerable.Range(0, 0).ToList(),
						name: "List Field"
					)
				}
			}
		};

		private static IEnumerable<FieldValueTestCases> FieldValueAreNullsTestSource = new[]
		{
			new FieldValueTestCases
			{
				FieldValues = new []
				{
					new FieldValueTestCase(
						value: null,
						name: "Null Field"
					)
				}
			},
		};

		private const int _MAX_COLLECTION_COUNT = 100000;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IObjectManagerFacade>();
			_loggerMock = new Mock<IAPILog>();
			_loggerMock.Setup(x => x.ForContext<ObjectManagerFacadeDiscoverHeavyRequestDecorator>())
				.Returns(_loggerMock.Object);

			_sut = new ObjectManagerFacadeDiscoverHeavyRequestDecorator(
				_objectManagerMock.Object,
				_loggerMock.Object);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
		public async Task CreateAsync_ShouldLogWarningWhenFieldCollectionCountExceededByOne(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Exceeded By One";
			const string operation = "CREATE";
			const string objectId = "[UNKNOWN]";
			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId,
				workspaceId,
				fieldValueTestCases.FieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
		public async Task CreateAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Is Exceeded Or Not";
			const string operation = "CREATE";
			const string objectId = "[UNKNOWN]";
			IEnumerable<FieldValueTestCase> exceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
			IEnumerable<FieldValueTestCase> notExceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId,
				workspaceId,
				exceededFieldValues);
			VerifyIfHeavyRequestIsNotLoggedIn(notExceededFieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
		public async Task CreateAsync_ShouldNotLogWarningWhenFieldCollectionCountEqualsMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Count Equals Max";
			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
		public async Task CreateAsync_ShouldNotLogWarningWhenFieldCollectionCountLowerThanMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Count Lower Than Max";
			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
		public async Task CreateAsync_ShouldNotLogWarningWhenFieldCollectionIsEmpty(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Is Empty";
			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreNullsTestSource))]
		public async Task CreateAsync_ShouldNotLogWarningWhenFieldIsNull(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Is Null";
			CreateRequest request = BuildCreateRequest(
				fieldValueTestCases.FieldValues,
				objectTypeName);

			CreateResult result = new CreateResult();
			SetupCreateAsyncInObjectManager(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
		public async Task ReadAsync_ShouldLogWarningWhenFieldCollectionCountExceededByOne(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "[UNKNOWN]";
			const string operation = "READ";
			const int objectId = 33;
			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId.ToString(),
				workspaceId,
				fieldValueTestCases.FieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
		public async Task ReadAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "[UNKNOWN]";
			const string operation = "READ";
			const int objectId = 33;

			IEnumerable<FieldValueTestCase> exceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
			IEnumerable<FieldValueTestCase> notExceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId.ToString(),
				workspaceId,
				exceededFieldValues);
			VerifyIfHeavyRequestIsNotLoggedIn(notExceededFieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
		public async Task ReadAsync_ShouldNotLogWarningWhenFieldCollectionCountEqualsMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;
			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
		public async Task ReadAsync_ShouldNotLogWarningWhenFieldCollectionCountLowerThanMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;
			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
		public async Task ReadAsync_ShouldNotLogWarningWhenFieldCollectionIsEmpty(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;
			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreNullsTestSource))]
		public async Task ReadAsync_ShouldNotLogWarningWhenFieldIsNull(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;
			ReadRequest request = BuildReadRequest(objectId);

			ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
			SetupReadAsyncInObjectManager(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
		public async Task UpdateAsync_ShouldLogWarningWhenFieldCollectionCountExceededByOne(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "[UNKNOWN]";
			const string operation = "UPDATE";
			const int objectId = 33;

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId.ToString(),
				workspaceId,
				fieldValueTestCases.FieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
		public async Task UpdateAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "[UNKNOWN]";
			const string operation = "UPDATE";
			const int objectId = 33;

			IEnumerable<FieldValueTestCase> exceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
			IEnumerable<FieldValueTestCase> notExceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId.ToString(),
				workspaceId,
				exceededFieldValues);
			VerifyIfHeavyRequestIsNotLoggedIn(notExceededFieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
		public async Task UpdateAsync_ShouldNotLogWarningWhenFieldCollectionCountEqualsMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
		public async Task UpdateAsync_ShouldNotLogWarningWhenFieldCollectionCountLowerThanMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
		public async Task UpdateAsync_ShouldNotLogWarningWhenFieldCollectionIsEmpty(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreNullsTestSource))]
		public async Task UpdateAsync_ShouldNotLogWarningWhenFieldIsNull(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const int objectId = 33;

			UpdateRequest request = BuildUpdateRequest(
				fieldValueTestCases.FieldValues,
				objectId);

			UpdateResult result = new UpdateResult();
			SetupUpdateAsyncInObjectManager(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
		public async Task QueryAsync_ShouldLogWarningWhenFieldCollectionCountExceededByOne(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Exceeded";
			const string operation = "QUERY";
			const string objectId = "[UNKNOWN]";
			const int start = 0;
			const int length = 1;

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId,
				workspaceId,
				fieldValueTestCases.FieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
		public async Task QueryAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Exceeded";
			const string operation = "QUERY";
			const string objectId = "[UNKNOWN]";
			const int start = 0;
			const int length = 1;

			IEnumerable<FieldValueTestCase> exceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
			IEnumerable<FieldValueTestCase> notExceededFieldValues = fieldValueTestCases
				.FieldValues
				.Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfHeavyRequestIsLoggedIn(
				operation,
				objectTypeName,
				objectId,
				workspaceId,
				exceededFieldValues);
			VerifyIfHeavyRequestIsNotLoggedIn(notExceededFieldValues);
		}

		[Test]
		[TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
		public async Task QueryAsync_ShouldNotLogWarningWhenFieldCollectionCountEqualsMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Not Exceeded";
			const int start = 0;
			const int length = 1;

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
		public async Task QueryAsync_ShouldNotLogWarningWhenFieldCollectionCountLowerThanMax(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Not Exceeded";
			const int start = 0;
			const int length = 1;

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
		public async Task QueryAsync_ShouldNotLogWarningWhenFieldCollectionIsEmpty(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Not Exceeded";
			const int start = 0;
			const int length = 1;

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		[TestCaseSource(nameof(FieldValueAreNullsTestSource))]
		public async Task QueryAsync_ShouldNotLogWarningWhenFieldIsNull(
			FieldValueTestCases fieldValueTestCases)
		{
			//arrange
			const int workspaceId = 101;
			const string objectTypeName = "Object Null";
			const int start = 0;
			const int length = 1;

			QueryRequest request = BuildQueryRequest(objectTypeName);

			QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
			SetupQueryAsyncInObjectManager(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		public async Task DeleteAsync_ShouldNotLogAnyWarning()
		{
			//arrange
			const int workspaceId = 101;

			DeleteRequest request = new DeleteRequest();
			DeleteResult result = new DeleteResult();
			SetupDeleteAsyncInObjectManager(result);

			//act
			DeleteResult actualResult = await _sut.DeleteAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
			VerifyIfNoneHeavyRequestIsNotLoggedIn();
		}

		[Test]
		public void This_ShouldDisposeObjectManagerFacade()
		{
			//act
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void This_ShouldDisposeObjectManagerFacadeOnlyOnce()
		{
			//act
			_sut.Dispose();
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Once);
		}

		private CreateRequest BuildCreateRequest(
			IEnumerable<FieldValueTestCase> fieldValueTestCases,
			string objectTypeName)
		{
			return new CreateRequest
			{
				FieldValues = fieldValueTestCases.Select(x => new FieldRefValuePair
				{
					Value = x.Value,
					Field = new FieldRef { Name = x.Name }
				}),
				ObjectType = new ObjectTypeRef { Name = objectTypeName }
			};
		}

		private UpdateRequest BuildUpdateRequest(
			IEnumerable<FieldValueTestCase> fieldValueTestCases,
			int objectId)
		{
			return new UpdateRequest
			{
				FieldValues = fieldValueTestCases.Select(x => new FieldRefValuePair
				{
					Value = x.Value,
					Field = new FieldRef { Name = x.Name }
				}),
				Object = new RelativityObjectRef { ArtifactID = objectId }
			};
		}

		private ReadRequest BuildReadRequest(int objectId)
		{
			return new ReadRequest
			{
				Object = new RelativityObjectRef
				{
					ArtifactID = objectId
				}
			};
		}

		private ReadResult BuildReadResult(IEnumerable<FieldValueTestCase> fieldValueTestCases)
		{
			return new ReadResult
			{
				Object = new RelativityObject
				{
					FieldValues = fieldValueTestCases.Select(x => new FieldValuePair
					{
						Value = x.Value,
						Field = new Field { Name = x.Name }
					}).ToList()
				}
			};
		}

		private QueryRequest BuildQueryRequest(string objectTypeName)
		{
			return new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Name = objectTypeName
				}
			};
		}

		private QueryResult BuildQueryResult(IEnumerable<FieldValueTestCase> fieldValueTestCases)
		{
			return new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						FieldValues = fieldValueTestCases.Select(x => new FieldValuePair
						{
							Value = x.Value,
							Field = new Field { Name = x.Name }
						}).ToList()
					}
				}
			};
		}

		private void SetupCreateAsyncInObjectManager(CreateResult result)
		{
			_objectManagerMock.Setup(x => x.CreateAsync(
					It.IsAny<int>(),
					It.IsAny<CreateRequest>()))
				.ReturnsAsync(result);
		}

		private void SetupUpdateAsyncInObjectManager(UpdateResult result)
		{
			_objectManagerMock.Setup(x => x.UpdateAsync(
					It.IsAny<int>(),
					It.IsAny<UpdateRequest>()))
				.ReturnsAsync(result);
		}

		private void SetupReadAsyncInObjectManager(ReadResult result)
		{
			_objectManagerMock.Setup(x => x.ReadAsync(
					It.IsAny<int>(),
					It.IsAny<ReadRequest>()))
				.ReturnsAsync(result);
		}

		private void SetupQueryAsyncInObjectManager(QueryResult result)
		{
			_objectManagerMock.Setup(x => x.QueryAsync(
					It.IsAny<int>(),
					It.IsAny<QueryRequest>(),
					It.IsAny<int>(),
					It.IsAny<int>()))
				.ReturnsAsync(result);
		}

		private void SetupDeleteAsyncInObjectManager(DeleteResult result)
		{
			_objectManagerMock.Setup(x => x.DeleteAsync(
					It.IsAny<int>(),
					It.IsAny<DeleteRequest>()))
				.ReturnsAsync(result);
		}

		private void VerifyIfNoneHeavyRequestIsNotLoggedIn()
		{
			_loggerMock.Verify(
				x => x.LogWarning(It.IsAny<string>()),
				Times.Never);
		}

		private void VerifyIfHeavyRequestIsNotLoggedIn(
			IEnumerable<FieldValueTestCase> notExceededTestCases)
		{
			foreach (var notExceededTestCase in notExceededTestCases)
			{
				_loggerMock.Verify(x => x.LogWarning(
						$"Requested field {notExceededTestCase.Name} exceeded max collection count"
						+ $" - {notExceededTestCase.Value.Count}, when allowed is {_MAX_COLLECTION_COUNT}")
					, Times.Never);
			}
		}

		private void VerifyIfHeavyRequestIsLoggedIn(
			string operation,
			string objectTypeName,
			string objectId,
			int workspaceId,
			IEnumerable<FieldValueTestCase> exceededTestCases)
		{
			_loggerMock.Verify(x => x.LogWarning(
				$"Heavy request discovered when executing {operation}"
				+ $" on object of type [{objectTypeName}], id {objectId} with ObjectManager"
				+ $" (Workspace: {workspaceId})")
				, Times.AtLeastOnce);
			foreach (var exceededTestCase in exceededTestCases)
			{
				_loggerMock.Verify(x => x.LogWarning(
					$"Requested field {exceededTestCase.Name} exceeded max collection count"
					+ $" - {exceededTestCase.Value.Count}, when allowed is {_MAX_COLLECTION_COUNT}")
					, Times.Once);
			}
		}
	}
}
