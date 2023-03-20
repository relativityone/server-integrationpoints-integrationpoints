using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using SystemInterface.IO;
using static kCura.IntegrationPoints.Core.Constants;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class JobControllerTests : TestsBase
    {
        private WorkspaceTest _destinationWorkspace;

        [SetUp]
        public void Setup()
        {
            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();
        }

        [IdentifiedTest("A1CDEE5D-5292-4B0C-9982-EE3679F757F8")]
        public void Run_ShouldScheduleJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            var response = sut.Run(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            FakeRelativityInstance.JobsInQueue.Single().RelatedObjectArtifactID.Should()
                .Be(integrationPoint.ArtifactId);
        }

        [IdentifiedTest("CCAFB6E8-9D1C-424F-BBBF-AE8A83F4A7AB")]
        public void Run_ShouldNotScheduleJobTwice()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            var response = sut.Run(payload);

            sut.Run(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            FakeRelativityInstance.JobsInQueue.Single().RelatedObjectArtifactID.Should()
                .Be(integrationPoint.ArtifactId);
        }

        [IdentifiedTest("2C155261-868D-4723-A7D5-4A9DC17C309A")]
        public void Retry_ShouldScheduleRetryJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);
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

        [IdentifiedTest("B7782A2E-2FA6-4CE8-AB8C-D3FB1115765E")]
        [TestCase(OverwriteModeNames.AppendOnlyModeName, true, OverwriteModeNames.AppendOverlayModeName)]
        [TestCase(OverwriteModeNames.AppendOnlyModeName, false, OverwriteModeNames.AppendOnlyModeName)]
        [TestCase(OverwriteModeNames.OverlayOnlyModeName, false, OverwriteModeNames.OverlayOnlyModeName)]
        [TestCase(OverwriteModeNames.AppendOverlayModeName, false, OverwriteModeNames.AppendOverlayModeName)]
        public void Retry_ShouldAssignCorrectOverwriteModeToJobHistory(string initialOverwriteMode, bool switchModeToAppendOverlay, string expectedOverwriteMode)
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);
            integrationPoint.HasErrors = true;
            ConvertToOverwriteChoice(integrationPoint, initialOverwriteMode);

            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(new JobTest(), integrationPoint);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/retry");

            // Act
            sut.Retry(payload, switchModeToAppendOverlay);

            // Assert
            JobHistoryTest addedJobHistoryForNextRun = SourceWorkspace.JobHistory.LastOrDefault();
            addedJobHistoryForNextRun.Overwrite.Should().Be(expectedOverwriteMode);
        }

        [IdentifiedTest("EEDDA654-F7C0-4843-BAF6-ADBDB57EFC22")]
        public void Retry_ShouldNotScheduleRetryJob_WhenIntegrationPointDoesNotHaveErrors()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);
            integrationPoint.HasErrors = false;

            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(new JobTest(), integrationPoint);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/retry");

            // Act
            sut.Retry(payload);

            // Assert
            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
        }

        [IdentifiedTest("B6CE1147-7484-42C9-A078-09742469D3EE")]
        public void Stop_ShouldStopRunningJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

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
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

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
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [IdentifiedTest("63B88DB8-C459-4608-A92E-B39E15EEA138")]
        public void Stop_ShouldNotStopUnstoppableJob()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

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
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [IdentifiedTest("63B88DB8-C459-4608-A92E-B39E15EEA138")]
        public void Stop_ShouldStopJobWithChildJobs()
        {
            // Arrange
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            var childJobs = Enumerable.Range(0, 5).Select(x =>
            {
                var childJob = new JobTest();
                childJob.RelatedObjectArtifactID = job.RelatedObjectArtifactID;
                childJob.WorkspaceID = job.WorkspaceID;
                childJob.JobDetails = job.JobDetails;
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
        public void Run_ShouldScheduleImportLoadFileJobWithLoadFileInfo()
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
            var response = sut.Run(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            JobTest job = FakeRelativityInstance.JobsInQueue.Single(x => x.RelatedObjectArtifactID == integrationPoint.ArtifactId);

            TaskParameters jobParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);

            LoadFileTaskParameters loadFileParameters = ((JObject)jobParameters.BatchParameters).ToObject<LoadFileTaskParameters>();

            loadFileParameters.Size.Should().Be(size);
            loadFileParameters.LastModifiedDate.Should().Be(modifiedDate);
        }

        [IdentifiedTest("7F3A7A24-AFE0-414F-A6AD-629A84218ED8")]
        public void Run_ShouldResultInValidationFailed()
        {
            // Arrange
            Proxy.PermissionManager.GrantNotConfiguredPermissions = false;

            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper
                .CreateWorkspaceWithIntegrationPointsApp(destinationWorkspaceArtifactId);
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
                .CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);

            JobController.Payload payload = new JobController.Payload
            {
                ArtifactId = integrationPoint.ArtifactId,
                AppId = SourceWorkspace.ArtifactId
            };

            JobController sut = PrepareSut(HttpMethod.Post, "/run");

            // Act
            HttpResponseMessage response = sut.Run(payload);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Should()
                .BeAssignableTo<ObjectContent<JobActionResult>>()
                .Which.Value.Should().BeAssignableTo<JobActionResult>()
                .Which.IsValid.Should().BeFalse();

            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
        }

        protected ClaimsPrincipal GetUserClaimsPrincipal() =>
            new ClaimsPrincipal(new[]
            {
                        new ClaimsIdentity(new[]
                        {
                            new Claim("rel_uai", User.ArtifactId.ToString())
                        })
            });
        private JobController PrepareSut(HttpMethod method, string requestUri)
        {
            JobController sut = Container.Resolve<JobController>();
            sut.User = GetUserClaimsPrincipal();

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            sut.Request = request;

            return sut;
        }

        private void ConvertToOverwriteChoice(IntegrationPointTest integrationPoint, string overwriteMode)
        {
            switch (overwriteMode)
            {
                case OverwriteModeNames.AppendOnlyModeName:
                    integrationPoint.OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOnly;
                    break;
                case OverwriteModeNames.AppendOverlayModeName:
                    integrationPoint.OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOverlay;
                    break;
                case OverwriteModeNames.OverlayOnlyModeName:
                    integrationPoint.OverwriteFields = OverwriteFieldsChoices.IntegrationPointOverlayOnly;
                    break;
            }
        }
    }
}
