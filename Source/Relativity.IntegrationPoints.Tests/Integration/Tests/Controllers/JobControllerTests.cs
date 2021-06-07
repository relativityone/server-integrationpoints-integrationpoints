using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Framework.Extensions;
using Relativity.Testing.Identification;
using SystemInterface.IO;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    [IdentifiedTestFixture("4FDA6BE8-7BA6-4755-A7AD-9C48FEB26877")]
    public class JobControllerTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;

        protected override WindsorContainer GetContainer()
        {
            var container = base.GetContainer();

            container.Register(Component.For<JobController>().ImplementedBy<JobController>());

            return container;
        }

        [SetUp]
        public void Setup()
        {
            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();
        }

        protected ClaimsPrincipal GetUserClaimsPrincipal() => new ClaimsPrincipal(new[]
            {new ClaimsIdentity(new[] {new Claim("rel_uai", User.ArtifactId.ToString())})});

        [IdentifiedTest("A1CDEE5D-5292-4B0C-9982-EE3679F757F8")]
        public async Task Run_ShouldScheduleJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            var response = await sut.Run(payload).ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            FakeRelativityInstance.JobsInQueue.Single().RelatedObjectArtifactID.Should()
                .Be(integrationPoint.ArtifactId);
        }

        [IdentifiedTest("CCAFB6E8-9D1C-424F-BBBF-AE8A83F4A7AB")]
        public async Task Run_ShouldNotScheduleJobTwice()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            var response = await sut.Run(payload).ConfigureAwait(false);
            var response2 = await sut.Run(payload).ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            FakeRelativityInstance.JobsInQueue.Single().RelatedObjectArtifactID.Should()
                .Be(integrationPoint.ArtifactId);
            response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [IdentifiedTest("2C155261-868D-4723-A7D5-4A9DC17C309A")]
        public void Retry_ShouldScheduleRetryJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);
            integrationPoint.HasErrors = true;

            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(new JobTest(), integrationPoint);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/retry");

            // Act
            var response = sut.Retry(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            FakeRelativityInstance.JobsInQueue.First().RelatedObjectArtifactID.Should().Be(integrationPoint.ArtifactId);
        }

        [IdentifiedTest("EEDDA654-F7C0-4843-BAF6-ADBDB57EFC22")]
        public void Retry_ShouldNotScheduleRetryJob_WhenIntegrationPointDoesNotHaveErrors()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);
            integrationPoint.HasErrors = false;

            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(new JobTest(), integrationPoint);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/retry");

            // Act
            var response = sut.Retry(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse();
            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
        }

        [IdentifiedTest("B6CE1147-7484-42C9-A078-09742469D3EE")]
        public void Stop_ShouldStopRunningJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            JobHistoryTest jobHistory =
                SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryProcessing;

            job.LastRunTime = Context.CurrentDateTime.AddMinutes(-1);
            job.LockedByAgentID = FakeRelativityInstance.Agents.First().ArtifactId;

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/stop");

            // Act
            var result = sut.Stop(payload);

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            job.StopState.Should().Be(StopState.Stopping);
        }

        [IdentifiedTest("54E564C5-342E-4864-866B-ECF9C3F464C8")]
        public void Stop_ShouldNotStopAlreadyStoppedJobJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            JobHistoryTest jobHistory =
                SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;

            job.LastRunTime = Context.CurrentDateTime.AddMinutes(-1);
            job.LockedByAgentID = FakeRelativityInstance.Agents.First().ArtifactId;

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/stop");

            // Act
            var result = sut.Stop(payload);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [IdentifiedTest("63B88DB8-C459-4608-A92E-B39E15EEA138")]
        public void Stop_ShouldNotStopUnstoppableJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            JobHistoryTest jobHistory =
                SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryProcessing;

            job.LastRunTime = Context.CurrentDateTime.AddMinutes(-1);
            job.LockedByAgentID = FakeRelativityInstance.Agents.First().ArtifactId;
            job.StopState = StopState.Unstoppable;

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/stop");

            // Act
            var result = sut.Stop(payload);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [IdentifiedTest("63B88DB8-C459-4608-A92E-B39E15EEA138")]
        public void Stop_ShouldStopJobWithChildJobs()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(_destinationWorkspace);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            var childJobs = Enumerable.Range(0, 5).Select(x =>
            {
                var childJob = job.Copy();
                childJob.ParentJobId = job.JobId;
                childJob.RootJobId = job.JobId;
                childJob.JobId = job.JobId + x + 1;

                return childJob;
            });

            FakeRelativityInstance.JobsInQueue.AddRange(childJobs);

            JobHistoryTest jobHistory =
                SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            jobHistory.JobStatus = JobStatusChoices.JobHistoryProcessing;

            job.LastRunTime = Context.CurrentDateTime.AddMinutes(-1);
            job.LockedByAgentID = FakeRelativityInstance.Agents.First().ArtifactId;
            job.StopState = StopState.Unstoppable;

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/stop");

            // Act
            var result = sut.Stop(payload);

            // Assert
            result.IsSuccessStatusCode.Should().BeTrue();

            FakeRelativityInstance.JobsInQueue.All(x => x.StopState == StopState.Stopping).Should().BeTrue();
        }

        [IdentifiedTest("610B7F40-F951-4FBC-A70F-C5AA4EBC65C4")]
        public async Task Run_ShouldScheduleImportLoadFileJobWithLoadFileInfo()
        {
            // Arrange
            const string loadFile = "DataTransfer\\Import\\SaltPepper\\saltvpepper-no_errors.dat";
            const long size = 1000;
            DateTime modifiedDate = new DateTime(2020, 1, 1);

            FakeFileInfoFactory fileInfoFactory = new FakeFileInfoFactory();

            fileInfoFactory.SetupFile(loadFile, size, modifiedDate);

            Container.Register(Component.For<IFileInfoFactory>().UsingFactoryMethod(c => fileInfoFactory)
                .LifestyleTransient().Named(nameof(FakeFileInfoFactory)).IsDefault());

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportDocumentLoadFileIntegrationPoint(loadFile);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            var response = await sut.Run(payload).ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            JobTest job = FakeRelativityInstance.JobsInQueue.Single(x => x.RelatedObjectArtifactID == integrationPoint.ArtifactId);

            TaskParameters jobParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);

            LoadFileTaskParameters loadFileParameters = ((JObject)jobParameters.BatchParameters).ToObject<LoadFileTaskParameters>();

            loadFileParameters.Size.Should().Be(size);
            loadFileParameters.LastModifiedDate.Should().Be(modifiedDate);
        }

        private JobController PrepareSut(HttpMethod method, string requestUri)
        {
            JobController sut = Container.Resolve<JobController>();
            sut.User = GetUserClaimsPrincipal();
            sut.Request = new HttpRequestMessage(method, requestUri);

            return sut;
        }
    }
}