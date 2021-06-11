using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [IdentifiedTestFixture("BA0C4AD6-6236-4235-BEBB-CB1084A978E9")]
    [TestExecutionCategory.CI, TestLevel.L1]
    public class SyncWorkerTests : TestsBase
    {
        private SyncWorker PrepareSut(Action<FakeJobImport> importAction)
        {
	        Container.Register(Component.For<IDataSourceProvider>()
		        .ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>()
		        .Named(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID));
        
            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction)).LifestyleSingleton());

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

        /// <summary>
        /// Creates <see cref="numberOfBatches"/> jobs in queue for the same IntegrationPoint and returns one of them
        /// </summary>
        private JobTest PrepareJobs(string xmlPath, int numberOfBatches)
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
		        FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace,
			        integrationPoint);

	        SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

	        TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
	        string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value)
		        .ToArray();

	        taskParameters.BatchParameters = recordsIds;

	        job.JobDetails = Serializer.Serialize(taskParameters);

	        for (int i = 1; i < numberOfBatches; i++)
	        {
		        JobTest newJob =
			        FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace,
				        integrationPoint);

		        newJob.JobDetails = job.JobDetails; // link all jobs together with BatchInstance
	        }

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
            SyncWorker sut = PrepareSut((importJob) =>
            {
                importJob.Complete(numberOfRecords);
            });

            // Act
            sut.Execute(new Job(job.AsDataRow()));

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
        }
        
        [IdentifiedTest("72118579-91DB-4018-8EF9-A4EB3FC2CD51")]
        public void SyncWorker_ShouldDrainStop()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int drainStopAfterImporting = 50;
            
            string xmlPath = PrepareRecords(numberOfRecords);
            JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();
            
            SyncWorker sut = PrepareSut((importJob) =>
            {
                importJob.Complete(drainStopAfterImporting);

                agent.ToBeRemoved = true;
            });

            // Act
            sut.Execute(new Job(job.AsDataRow()));
            
            // Assert
            List<string> remainingItems = GetRemainingItems(job);

            remainingItems.Count.Should().Be(numberOfRecords - drainStopAfterImporting);
            remainingItems.Should().BeEquivalentTo(Enumerable.Range(drainStopAfterImporting, numberOfRecords - drainStopAfterImporting).Select(x => x.ToString()));
            
            jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
            jobHistory.ItemsTransferred.Should().Be(drainStopAfterImporting);
            job.StopState.Should().Be(StopState.DrainStopped);
        }

        [IdentifiedTest("4D867717-3C3D-4763-9E29-63AAAA435885")]
        public void SyncWorker_ShouldNotDrainStopOtherBatches()
        {
	        // Arrange
	        const int numberOfRecords = 100;
	        const int drainStopAfterImporting = 50;
	        const int numberOfBatches = 3;

	        string xmlPath = PrepareRecords(numberOfRecords);
	        JobTest job = PrepareJobs(xmlPath, numberOfBatches);
	        FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

	        SyncWorker sut = PrepareSut((importJob) =>
	        {
		        importJob.Complete(drainStopAfterImporting);

		        agent.ToBeRemoved = true;
	        });


	        // Act
	        var syncManagerJob = new Job(job.AsDataRow());
	        sut.Execute(syncManagerJob);

	        // Assert
	        job.StopState.Should().Be(StopState.DrainStopped);
	        FakeRelativityInstance.JobsInQueue.Where(x => x.JobId != job.JobId).All(x => x.StopState == StopState.None).Should().BeTrue();
        }

        [IdentifiedTest("A36C3183-BC7D-422A-AF55-F57814897E83")]
        public void SyncWorker_ShouldResumeDrainStoppedJob()
        {
	        // Arrange
	        const int numberOfRecords = 100;
	        const int numberOfErrors= 10;
	        const int initialTransferredItems = 50;
	        const int initialErroredItems = 50;

	        string xmlPath = PrepareRecords(numberOfRecords);
	        JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
	        FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

            jobHistory.JobStatus = new ChoiceRef(new List<Guid> { JobStatusChoices.JobHistorySuspendedGuid });
            jobHistory.ItemsTransferred = initialTransferredItems;
            jobHistory.ItemsWithErrors = initialErroredItems;

            jobHistory.TotalItems = numberOfErrors + numberOfRecords + initialErroredItems + initialTransferredItems;

            job.StopState = StopState.DrainStopped;

            SyncWorker sut = PrepareSut((importJob) =>
            {
	            importJob.Complete(numberOfRecords, numberOfErrors);
            });

            // Act
            var syncManagerJob = new Job(job.AsDataRow());
	        sut.Execute(syncManagerJob);

	        // Assert
	        FakeRelativityInstance.JobsInQueue.Single(x => x.JobId != job.JobId).StopState.Should().Be(StopState.None);
	        jobHistory.JobStatus.Guids.First().Should().Be(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);
	        jobHistory.ItemsTransferred.Should().Be(initialTransferredItems + numberOfRecords);
	        jobHistory.ItemsWithErrors.Should().Be(initialErroredItems + numberOfErrors);
        }

        private List<string> GetRemainingItems(JobTest job)
        {
            TaskParameters paramaters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            List<string> remainingItems = (paramaters.BatchParameters as JArray).ToObject<List<string>>();
            return remainingItems;
        }
    }
}