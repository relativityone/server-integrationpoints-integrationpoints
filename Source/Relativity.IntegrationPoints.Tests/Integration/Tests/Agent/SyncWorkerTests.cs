using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.ImportAPI;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [IdentifiedTestFixture("BA0C4AD6-6236-4235-BEBB-CB1084A978E9")]
    [TestExecutionCategory.CI, TestLevel.L1]
    public class SyncWorkerTests : TestsBase
    {
        private SyncWorker PrepareSut(int numberOfRecords)
        {
            Container.Register(Component.For<IDataSourceProvider>()
                .ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>().IsDefault());

            Container.Register(Component.For<IObjectTypeRepository>().Instance(
                new ObjectTypeRepository(SourceWorkspace.ArtifactId, Container.Resolve<IServicesMgr>(),
                    Container.Resolve<IHelper>(), Container.Resolve<IRelativityObjectManager>())));

            Container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizer>().Named(typeof(RdoSynchronizer).AssemblyQualifiedName).LifestyleTransient());
            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) =>
            {
                importJob.Complete(numberOfRecords);
            })).LifestyleSingleton());
            Container.Register(Component.For<kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI.IImportApiFacade>().ImplementedBy<FakeImportApiFacade>());
            
            SyncWorker sut = Container.Resolve<SyncWorker>();
            return sut;
        }

        private JobTest PrepareJob(string xmlPath, out JobHistoryTest jobHistory)
        {
            FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            SourceProviderTest provider =
                SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
            jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value).ToArray();

            taskParameters.BatchParameters = recordsIds;

            job.JobDetails = Serializer.Serialize(taskParameters);
            
            return job;
        }

        private string PrepareRecords(int numberOfRecords)
        {
            string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
            string tmpPath = Path.GetTempFileName();
            File.WriteAllText(tmpPath, xml);
            return tmpPath;
        }


        [IdentifiedTest("BCF72894-224F-4DB7-985F-0C53C93D153D")]
        public void SyncWorker_ShouldImportData()
        {
            // Arrange
            const int numberOfRecords = 100;
            string xmlPath = PrepareRecords(numberOfRecords);
            JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
            SyncWorker sut = PrepareSut(numberOfRecords);

            // Act
            sut.Execute(new Job(job.AsDataRow()));

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
        }
    }
}