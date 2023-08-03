using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
    public class RdoEntitySynchronizerTests : TestBase
    {
        private string _settings;
        private Mock<IHelper> _helper;
        private Mock<IRelativityFieldQuery> _fieldQuery;
        private Mock<IDiagnosticLog> _diagnosticLogMock;
        private IImportJobFactory _importJobFactory;

        public static IImportApiFactory GetMockAPI(IRelativityFieldQuery query)
        {
            var import = new Mock<Relativity.ImportAPI.IImportAPI>();
            List<RelativityObject> fields = query.GetFieldsForRdo(0);
            var list = new List<Relativity.ImportAPI.Data.Field>();
            MethodInfo methodInfo = typeof(Relativity.ImportAPI.Data.Field).GetProperty("ArtifactID").GetSetMethod(true);

            foreach (RelativityObject field in fields)
            {
                var newField = new Relativity.ImportAPI.Data.Field();
                methodInfo.Invoke(newField, new object[] { field.ArtifactID });
                list.Add(newField);
            }

            import.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(list);

            Mock<IImportApiFactory> mock = new Mock<IImportApiFactory>();
            mock.Setup(x => x.GetImportAPI()).Returns(import.Object);

            mock.Setup(x => x.GetImportApiFacade())
                .Returns(new ImportApiFacade(mock.Object, new Mock<ILogger<ImportApiFacade>>().Object));

            return mock.Object;
        }

        [OneTimeSetUp]
        public override void FixtureSetUp()
        {
            base.FixtureSetUp();

            _settings = JsonConvert.SerializeObject(new ImportSettings(new DestinationConfiguration()));
            _importJobFactory = new ImportJobFactory(Mock.Of<IMessageService>(), new EmptyLogger());
        }

        [SetUp]
        public override void SetUp()
        {
            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<RdoEntitySynchronizer>()).Returns(logger.Object);
            logger.Setup(x => x.ForContext<RdoSynchronizer>()).Returns(logger.Object);
            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(logger.Object);
            _helper = new Mock<IHelper>();
            _helper.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);
            _fieldQuery = new Mock<IRelativityFieldQuery>();
            _diagnosticLogMock = new Mock<IDiagnosticLog>();
        }

        [Test]
        public void GetFields_FieldsContainsFirstName_MakesFieldRequired()
        {
            // ARRANGE
            List<RelativityObject> fields = PrepareFields("Test1", EntityFieldGuids.FirstName);
            _fieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(fields);

            // ACT
            RdoSynchronizer sync = PrepareSut();
            IEnumerable<FieldEntry> actualFields = sync.GetFields(new DataSourceProviderConfiguration(_settings));

            // ASSERT
            FieldEntry field = actualFields.First(x => x.DisplayName.Equals("Test1"));
            Assert.AreEqual(true, field.IsRequired);
        }

        [Test]
        public void GetFields_FieldsContainsLastName_MakesFieldRequired()
        {
            // ARRANGE
            List<RelativityObject> fields = PrepareFields("Test1", EntityFieldGuids.LastName);
            _fieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(fields);

            // ACT
            RdoSynchronizer sync = PrepareSut();
            IEnumerable<FieldEntry> actualFields = sync.GetFields(new DataSourceProviderConfiguration(_settings));

            // ASSERT
            FieldEntry field = actualFields.First(x => x.DisplayName.Equals("Test1"));
            Assert.AreEqual(true, field.IsRequired);
        }

        [Test]
        public void GetFields_FieldsContainsUniqueID_OnlyUniqueIDSetForIdentifier()
        {
            // ARRANGE
            List<RelativityObject> fields = PrepareFields("Test1", EntityFieldGuids.UniqueID);
            _fieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(fields);

            // ACT
            RdoSynchronizer sync = PrepareSut();
            List<FieldEntry> actualFields = sync.GetFields(new DataSourceProviderConfiguration(_settings)).ToList();

            // ASSERT
            int idCount = actualFields.Count(x => x.IsIdentifier);
            Assert.AreEqual(1, idCount);
            FieldEntry field = actualFields.Single(x => x.DisplayName.Equals("Test1"));
            Assert.AreEqual(true, field.IsIdentifier);
        }

        private List<RelativityObject> PrepareFields(string fieldName, string fieldGuid)
        {
            return new List<RelativityObject>
            {
                new RelativityObject
                {
                    ArtifactID = 1,
                    Guids = new List<Guid>
                    {
                        Guid.Empty
                    },
                    Name = string.Empty
                },
                new RelativityObject
                {
                    ArtifactID = 2,
                    Guids = new List<Guid>
                    {
                        Guid.Parse(fieldGuid)
                    },
                    Name = fieldName
                }
            };
        }

        private RdoEntitySynchronizer PrepareSut()
        {
            Mock<IEntityManagerLinksSanitizer> entityManagerLinksSanitizer = new Mock<IEntityManagerLinksSanitizer>();
            return new RdoEntitySynchronizer(
                _fieldQuery.Object,
                GetMockAPI(_fieldQuery.Object),
                _importJobFactory,
                _helper.Object,
                entityManagerLinksSanitizer.Object,
                _diagnosticLogMock.Object,
                Serializer);
        }
    }
}
