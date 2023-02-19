using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests
{
    [TestFixture, Category("Unit")]
    public class RelativityReaderBaseTests : TestBase
    {
        private const string _TOKEN_FOR_MORE = "There are some more things to get";

        [SetUp]
        public override void SetUp()
        {
        }

        protected ArtifactDTO[] ExecuteQueryToGetInitialResult()
        {
            return new ArtifactDTO[]
            {
                new ArtifactDTO(1,10, string.Empty, new ArtifactFieldDTO[0]),
                new ArtifactDTO(10,10, string.Empty, new ArtifactFieldDTO[0]),
            };
        }

        [Test]
        public void ReadDocumentCounterGetUpdated()
        {
            MockRelativityReaderBase instance = new MockRelativityReaderBase(ExecuteQueryToGetInitialResult, () => true);
            Assert.AreEqual(0, instance.Counter);

            instance.Read();
            instance.Read();

            Assert.AreEqual(2, instance.Counter);
        }

        [Test]
        public void ReadDocumentsUntilTheEnd_FetchMoreData()
        {
            MockRelativityReaderBase instance = new MockRelativityReaderBase(ExecuteQueryToGetInitialResult, () => false);

            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(3, instance.Counter);
            Assert.AreEqual(false, instance.IsClosed);
        }

        [Test]
        public void ReadDocumentsUntilTheEnd_NoMoreData_TheReaderClose()
        {
            MockRelativityReaderBase instance = new MockRelativityReaderBase(ExecuteQueryToGetInitialResult, () => true);

            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(false, instance.Read());
            Assert.AreEqual(true, instance.IsClosed);
        }

        [Test]
        public void ReadDocumentsUntilTheEnd_FetchMoreDataAndReadToEnd()
        {
            int fetchCount = 0;
            MockRelativityReaderBase instance = new MockRelativityReaderBase(ExecuteQueryToGetInitialResult, () =>
            {
                fetchCount++;
                return fetchCount > 1;
            });

            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(false, instance.Read());
            Assert.AreEqual(true, instance.IsClosed);
        }

        [Test]
        public void ReadDocumentsUntilTheEnd_FetchMoreDataFail()
        {
            int fetchCount = 0;
            MockRelativityReaderBase instance = new MockRelativityReaderBase(
                () => fetchCount == 0 ? this.ExecuteQueryToGetInitialResult() : null,
                () => true);

            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(true, instance.Read());
            Assert.AreEqual(false, instance.Read());
            Assert.AreEqual(true, instance.IsClosed);
            Assert.AreEqual(2, instance.Counter);
        }

        private class MockRelativityReaderBase : RelativityReaderBase
        {
            private readonly Func<ArtifactDTO[]> _fetchFunction;
            private readonly Func<bool> _allArtifactsFetchedFunc;

            public MockRelativityReaderBase(Func<ArtifactDTO[]> fetchFunction, Func<bool> allArtifactsFetchedFunc)
                : base(new[] { new DataColumn("Testing") })
            {
                _fetchFunction = fetchFunction;
                _allArtifactsFetchedFunc = allArtifactsFetchedFunc;
            }

            public int Counter { get { return ReadEntriesCount; } }

            public override string GetName(int i)
            {
                throw new NotImplementedException();
            }

            public override int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public override DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            public override string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public override Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public override object GetValue(int i)
            {
                throw new NotImplementedException();
            }

            protected override ArtifactDTO[] FetchArtifactDTOs()
            {
                return _fetchFunction.Invoke();
            }

            protected override bool AllArtifactsFetched()
            {
                return _allArtifactsFetchedFunc.Invoke();
            }
        }
    }
}
