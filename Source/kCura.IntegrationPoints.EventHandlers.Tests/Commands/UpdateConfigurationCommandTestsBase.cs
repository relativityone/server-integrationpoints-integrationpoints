using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public abstract class UpdateConfigurationCommandTestsBase : TestBase
    {
        private Mock<IExportQueryResult> _exportQueryResultFake;

        protected Mock<IEHHelper> EHHelperFake { get; set; }
        protected Mock<IRelativityObjectManager> RelativityObjectManagerMock { get;set; }
        protected Mock<IObjectManager> ObjectManagerMock { get; set; }

        protected abstract List<string> Names { get; }

        public override void SetUp()
        {
            _exportQueryResultFake = new Mock<IExportQueryResult>();

            RelativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            RelativityObjectManagerMock.Setup(x => x.QueryWithExportAsync(
                It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<ExecutionIdentity>())).ReturnsAsync(_exportQueryResultFake.Object);

            ObjectManagerMock = new Mock<IObjectManager>();
            ObjectManagerMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdatePerObjectsRequest>()))
                .Returns(Task.FromResult(new MassUpdateResult()));

            Mock<IServicesMgr> servicesMgrFake = new Mock<IServicesMgr>();
            servicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(ObjectManagerMock.Object);

            IAPILog log = Substitute.For<IAPILog>();

            Mock<ILogFactory> logFactoryFake = new Mock<ILogFactory>();
            logFactoryFake.Setup(x => x.GetLogger()).Returns(log);

            EHHelperFake = new Mock<IEHHelper>();
            EHHelperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactoryFake.Object);
            EHHelperFake.Setup(x => x.GetServicesManager()).Returns(servicesMgrFake.Object);
        }

        protected void ShouldNotBeUpdated()
        {
            ObjectManagerMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdatePerObjectsRequest>()), Times.Never);
        }

        protected void ShouldBeUpdated(RelativityObjectSlim objectSlim)
        {
            ObjectManagerMock.Verify(m => m.UpdateAsync(It.IsAny<int>(),
                It.IsAny<MassUpdatePerObjectsRequest>()), Times.Once);

            ObjectManagerMock.Verify(m => m.UpdateAsync(It.IsAny<int>(),
                It.Is<MassUpdatePerObjectsRequest>(x => x.ObjectValues[0].Values.SequenceEqual(objectSlim.Values))),
                Times.Once);
        }

        protected virtual void SetupRead(RelativityObjectSlim value)
        {
            _exportQueryResultFake.Setup(x => x.ExportResult).Returns(new ExportInitializationResults
            {
                FieldData = Names.Select(x => new FieldMetadata { Name = x }).ToList()
            });

            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<int>()))
                .ReturnsAsync(new List<RelativityObjectSlim> { value });

            RelativityObjectManagerMock.Setup(x => x.Query<SourceProvider>(It.IsAny<QueryRequest>(), ExecutionIdentity.System))
                .Returns(new List<SourceProvider> { new SourceProvider() });
        }
    }
}