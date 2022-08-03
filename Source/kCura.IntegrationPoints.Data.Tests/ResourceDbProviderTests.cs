using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests
{
    [TestFixture, Category("Unit")]
    public class ResourceDbProviderTests
    {
        private IResourceDbProvider _sut;
        private Mock<IDBContext> _dbContextMock;
        private Mock<IHelper> _helperMock;

        private const int _WORKSPACE_ID = 1000123;

        [SetUp]
        public void SetUp()
        {
            _dbContextMock = new Mock<IDBContext>();
            _helperMock = new Mock<IHelper>();
            _helperMock.Setup(x => x.GetDBContext(_WORKSPACE_ID)).Returns(_dbContextMock.Object);
            _sut = new ResourceDbProvider(_helperMock.Object);
        }

        [Test]
        public void GetSchemalessResourceDataBasePrepend_ShouldCallHelperAndReturnProperValue()
        {
            // arrange
            const string expectedResult = "SCHEMALESS_RESOURCE_DATA_BASE_PREPEND";
            _helperMock.Setup(x => x.GetSchemalessResourceDataBasePrepend(_dbContextMock.Object)).Returns(expectedResult);

            // act
            string actualResult = _sut.GetSchemalessResourceDataBasePrepend(_WORKSPACE_ID);

            // assert
            _helperMock.Verify(x => x.GetDBContext(_WORKSPACE_ID), Times.Once);
            _helperMock.Verify(x => x.GetSchemalessResourceDataBasePrepend(_dbContextMock.Object), Times.Once);
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public void GetResourceDbPrepend_ShouldCallHelperAndReturnProperValue()
        {
            // arrange
            const string expectedResult = "RESOURCE_DB_PREPEND";
            _helperMock.Setup(x => x.ResourceDBPrepend(_dbContextMock.Object)).Returns(expectedResult);

            // act
            string actualResult = _sut.GetResourceDbPrepend(_WORKSPACE_ID);

            // assert
            _helperMock.Verify(x => x.GetDBContext(_WORKSPACE_ID), Times.Once);
            _helperMock.Verify(x => x.ResourceDBPrepend(_dbContextMock.Object), Times.Once);
            actualResult.Should().Be(expectedResult);
        }
    }
}
