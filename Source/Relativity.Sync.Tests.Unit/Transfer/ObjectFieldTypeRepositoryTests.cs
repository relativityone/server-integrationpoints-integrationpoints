using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    public class DocumentFieldRepositoryTests
    {
        private ObjectFieldTypeRepository _instance;
        private Mock<IObjectManager> _objectManager;
        private readonly int _sourceWorkspaceArtifactId = 1234;
        private const int _RDO_ARTIFACT_TYPE_ID = 420;

        [SetUp]
        public void SetUp()
        {
            _objectManager = new Mock<IObjectManager>();

            var serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            serviceFactoryForUser.Setup(f => f.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _instance = new ObjectFieldTypeRepository(serviceFactoryForUser.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldReturnFieldWithMappedRelativityType()
        {
            // Arrange
            string field1Name = "Field 1";
            RelativityDataType field1RelativityDataType = RelativityDataType.Date;

            string field2Name = "Field 2";
            RelativityDataType field2RelativityDataType = RelativityDataType.LongText;

            List<string> fieldNames = new List<string> { field1Name, field2Name };

            List<RelativityObjectSlim> returnObjects = new List<RelativityObjectSlim>
            {
                new RelativityObjectSlim { Values = new List<object> { field1Name, field1RelativityDataType.GetDescription() } },
                new RelativityObjectSlim { Values = new List<object> { field2Name, field2RelativityDataType.GetDescription() } }
            };

            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnObjects };

            const int start = 0;
            _objectManager.Setup(om => om.QuerySlimAsync(_sourceWorkspaceArtifactId, It.IsAny<QueryRequest>(), start, fieldNames.Count, CancellationToken.None)).ReturnsAsync(queryResult);

            // Act
            IDictionary<string, RelativityDataType> result = await _instance
                .GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, fieldNames, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Count.Should().Be(fieldNames.Count);
            result.Should().ContainKey(field1Name).WhichValue.Should().Be(field1RelativityDataType);
            result.Should().ContainKey(field2Name).WhichValue.Should().Be(field2RelativityDataType);
        }

        [Test]
        public async Task ItShouldCorrectlyEncodeFieldNamesInRequest()
        {
            // Arrange
            List<string> requestedFieldNames = new List<string>
            {
                "Cool Field Name",     // spaces
                "Commas, Hello",       // comma
                "Colon: A True Story", // colon
                "Sync's Cool Field",   // single quote - should escape
                "Nice \\ Field"        // backslash - should escape
            };
            IEnumerable<string> returnedFieldNames = requestedFieldNames;

            List<RelativityObjectSlim> returnedObjects = returnedFieldNames.Select(GenerateObjectSlimFromFieldName).ToList();
            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnedObjects };
            SetupAnyQuerySlimAsync()
                .ReturnsAsync(queryResult);

            // Act
            await _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, requestedFieldNames, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            const string expectedFieldNameArray =
                "'Cool Field Name', 'Commas, Hello', 'Colon: A True Story', 'Sync\\'s Cool Field', 'Nice \\\\ Field'";
            _objectManager.Verify(x => x.QuerySlimAsync(It.IsAny<int>(),
                It.Is<QueryRequest>(q => ConditionContainsFieldNameArray(q, expectedFieldNameArray)),
                It.IsAny<int>(),
                It.IsAny<int>(),
                CancellationToken.None), Times.AtLeastOnce);
        }

        private static IEnumerable<TestCaseData> EmptyFieldNamesListTestCases
        {
            get
            {
                yield return new TestCaseData((object)null);
                yield return new TestCaseData((object)Array.Empty<string>());
            }
        }

        [TestCaseSource(nameof(EmptyFieldNamesListTestCases))]
        public async Task ItShouldReturnEmptyDictionaryWhenFieldNamesListIsNullOrEmpty(ICollection<string> fieldNames)
        {
            // Act
            IDictionary<string, RelativityDataType> result =
                await _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, fieldNames, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
            _objectManager.Verify(om => om.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ItShouldThrowKeplerServiceExceptionWhenObjectManagerThrows()
        {
            // Arrange
            List<string> fieldNames = new List<string> { "test" };
            SetupAnyQuerySlimAsync()
                .Throws<ServiceException>();

            // Act
            Func<Task<IDictionary<string, RelativityDataType>>> action = () =>
                _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, fieldNames, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<SyncKeplerException>().ConfigureAwait(false);
        }

        [Test]
        public async Task ItShouldThrowFieldNotFoundExceptionWhenReturnedObjectsListIsNull()
        {
            // Arrange
            List<string> requestedFieldNames = new List<string> { "Cool Field Name", "Slick Field Name", "Dope Field Name" };

            List<RelativityObjectSlim> returnedObjects = null;
            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnedObjects };
            SetupAnyQuerySlimAsync()
                .ReturnsAsync(queryResult);

            // Act
            Func<Task<IDictionary<string, RelativityDataType>>> action = () =>
                _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, requestedFieldNames, CancellationToken.None);

            // Assert
            (await action.Should().ThrowAsync<FieldNotFoundException>().ConfigureAwait(false))
                .Which.Message.Should().ContainAll(requestedFieldNames);
        }

        [Test]
        public async Task ItShouldThrowFieldNotFoundExceptionWhenReturnedObjectsListIsEmpty()
        {
            // Arrange
            List<string> requestedFieldNames = new List<string> { "Cool Field Name", "Slick Field Name", "Dope Field Name" };

            List<RelativityObjectSlim> returnedObjects = new List<RelativityObjectSlim>();
            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnedObjects };
            SetupAnyQuerySlimAsync()
                .ReturnsAsync(queryResult);

            // Act
            Func<Task<IDictionary<string, RelativityDataType>>> action = () =>
                _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, requestedFieldNames, CancellationToken.None);

            // Assert
            (await action.Should().ThrowAsync<FieldNotFoundException>().ConfigureAwait(false))
                .Which.Message.Should().MatchRegex(": Cool Field Name, Slick Field Name, Dope Field Name$");
        }

        [Test]
        public async Task ItShouldThrowFieldNotFoundExceptionWhenFewerObjectsAreReturnedThanExpected()
        {
            // Arrange
            List<string> requestedFieldNames = new List<string> { "Cool Field Name", "Slick Field Name", "Dope Field Name", "Jazzy Field Name" };
            List<string> returnedFieldNames = new List<string> { "Cool Field Name", "Dope Field Name" };

            List<RelativityObjectSlim> returnedObjects = returnedFieldNames.Select(GenerateObjectSlimFromFieldName).ToList();
            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnedObjects };
            SetupAnyQuerySlimAsync()
                .ReturnsAsync(queryResult);

            // Act
            Func<Task<IDictionary<string, RelativityDataType>>> action = () =>
                _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, requestedFieldNames, CancellationToken.None);

            // Assert
            (await action.Should().ThrowAsync<FieldNotFoundException>().ConfigureAwait(false))
                .Which.Message.Should().MatchRegex(": Slick Field Name, Jazzy Field Name$");
        }

        [Test]
        public async Task ItShouldRequestUniqueInputFieldNames()
        {
            // Arrange
            List<string> requestedFieldNames = new List<string> { "Cool Field Name", "Slick Field Name", "Dope Field Name", "Slick Field Name", "Dope Field Name" };
            IEnumerable<string> returnedFieldNames = requestedFieldNames.Distinct();

            List<RelativityObjectSlim> returnedObjects = returnedFieldNames.Select(GenerateObjectSlimFromFieldName).ToList();
            QueryResultSlim queryResult = new QueryResultSlim { Objects = returnedObjects };
            SetupAnyQuerySlimAsync()
                .ReturnsAsync(queryResult);

            // Act
            await _instance.GetRelativityDataTypesForFieldsByFieldNameAsync(_sourceWorkspaceArtifactId, _RDO_ARTIFACT_TYPE_ID, requestedFieldNames, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            _objectManager.Verify(x => x.QuerySlimAsync(It.IsAny<int>(),
                It.Is<QueryRequest>(q => ConditionContainsExactFields(q, requestedFieldNames.Distinct())),
                It.IsAny<int>(),
                requestedFieldNames.Distinct().Count(),
                CancellationToken.None), Times.AtLeastOnce);
        }

        private ISetup<IObjectManager, Task<QueryResultSlim>> SetupAnyQuerySlimAsync()
        {
            return _objectManager.Setup(om => om.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(),
                It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None));
        }

        private static RelativityObjectSlim GenerateObjectSlimFromFieldName(string fieldName)
        {
            return new RelativityObjectSlim { Values = new List<object> { fieldName, RelativityDataType.Currency.GetDescription() } };
        }

        // Performing assertions & then returning true makes any failure easier to locate.
        // This should be changed if we would verify over more than one invocation.
        private static bool ConditionContainsFieldNameArray(QueryRequest queryRequest, string fieldNames)
        {
            queryRequest.Condition.Should().Contain($"'Name' IN [{fieldNames}]");
            return true;
        }

        // Performing assertions & then returning true makes any failure easier to locate.
        // This should be changed if we would verify over more than one invocation.
        private static bool ConditionContainsExactFields(QueryRequest queryRequest, IEnumerable<string> fieldNames)
        {
            string condition = queryRequest.Condition;
            foreach (string field in fieldNames)
            {
                condition.Should()
                    .Contain(field).And
                    .Match(c => c.IndexOf(field, StringComparison.InvariantCulture) == c.LastIndexOf(field, StringComparison.InvariantCulture));
            }

            return true;
        }
    }
}
