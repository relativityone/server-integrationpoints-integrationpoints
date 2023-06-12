using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit")]
    public class ImportTransferDataContextTests : TestBase
    {
        private IDataReader _dataReader;
        private ImportTransferDataContext _instance;

        [SetUp]
        public override void SetUp()
        {
            _dataReader = Substitute.For<IDataReader>();
            IDataReaderFactory dataReaderFactory = Substitute.For<IDataReaderFactory>();
            dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), Arg.Any<string>(), Arg.Any<IJobStopManager>()).Returns(_dataReader);

            _instance = new ImportTransferDataContext(dataReaderFactory, string.Empty, new List<FieldMap>(), Arg.Any<IJobStopManager>());
        }

        [Test]
        public void ItShouldReturnDataReaderAssignedInConstructor()
        {
            // Assert
            Assert.AreSame(_dataReader, _instance.DataReader);
        }

        [Test]
        public void ItShouldCallDisposeOnDataReader()
        {
            _instance.Dispose();

            _dataReader.Received(1).Dispose();
        }

        [Test]
        public void TestTotalItemsFoundProperty()
        {
            _instance.TotalItemsFound = 42;
            Assert.AreEqual(42, _instance.TotalItemsFound);
        }
    }
}
