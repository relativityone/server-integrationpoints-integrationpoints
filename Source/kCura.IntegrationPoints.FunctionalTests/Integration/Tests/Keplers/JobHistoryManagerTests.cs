using FluentAssertions;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;
using Relativity.Testing.Identification;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Constants;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class JobHistoryManagerTests : TestsBase
    {
        private IJobHistoryManager _manager;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _manager = Container.Resolve<IJobHistoryManager>();
            AddTestData();
        }

        [IdentifiedTest("21FECB87-6307-43E9-900A-9119C94380DC")]
        public async Task ItShouldReturnJobHistoryWithCorrectValues()
        {
            //Arrange
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 0,
                PageSize = 5
            };
            int expectedAvailableItems = SourceWorkspace.JobHistory.Count();
            int expectedTransferredItems = SourceWorkspace.JobHistory.Sum(x => x.ItemsTransferred) ?? 0;

            //Act
            JobHistorySummaryModel result = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.TotalAvailable.Should().Be(expectedAvailableItems);
            result.TotalDocumentsPushed.Should().Be(expectedTransferredItems);
            result.Data.Length.Should().Be(expectedAvailableItems);
        }

        [IdentifiedTest("8C86BD4C-0BFD-4B01-A36E-5F3625EEF113")]
        public async Task JobHistoryShouldBeReturnedDespiteTooLargeVolumeDeclaredInRequest()
        {
            //Arrange
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 15,
                PageSize = 15
            };           

            //Act
            JobHistorySummaryModel result = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();            
            result.Data.Should().BeEmpty();
        }

        [IdentifiedTest("90EC58F4-B25D-4CD6-8000-013D95A0DEDB")]
        public async Task JobHistoryShouldBeReturnedAccordingToPageAndPageSizeConditions()
        {
            //Arrange
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 1,
                PageSize = 2
            };
            int expectedIndexOfFirstReturnedDataSet = request.Page * request.PageSize;
            int expectedIndexOfLastReturnedDataSet = expectedIndexOfFirstReturnedDataSet + request.PageSize - 1;
            int expectedNumberOfReturnedDataSets = request.PageSize;

            List<JobHistoryTest> sourceData = SourceWorkspace.JobHistory.Skip(expectedIndexOfFirstReturnedDataSet).Take(expectedNumberOfReturnedDataSets).ToList();

            //Act
            JobHistorySummaryModel result = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Data.Length.Should().Be(expectedNumberOfReturnedDataSets);

            for(int i = 0; i < result.Data.Length; i++)
            {
                result.Data[i].ItemsTransferred.Should().Be(sourceData[i].ItemsTransferred);
            }
        }

        //TODO: add sorting test at least for one property




        private void AddTestData()
        {
            string destination = SetupDestinationWorkspace();
            IntegrationPointTest integrationPoint = new IntegrationPointTest
            {
                SourceProvider = 100048,
                DestinationProvider = 100049
            };

            SetupNewJobHistoryTestData(integrationPoint, destination, itemsTransferred: 17, itemsTotal: 20, JobStatusChoices.JobHistoryCompletedWithErrors);
            SetupNewJobHistoryTestData(integrationPoint, destination, itemsTransferred: 7, itemsTotal: 7, JobStatusChoices.JobHistoryCompleted);
            SetupNewJobHistoryTestData(integrationPoint, destination, itemsTransferred: 10, itemsTotal: 12, JobStatusChoices.JobHistoryCompletedWithErrors);
            SetupNewJobHistoryTestData(integrationPoint, destination, itemsTransferred: 2, itemsTotal: 2, JobStatusChoices.JobHistoryCompleted);
            SetupNewJobHistoryTestData(integrationPoint, destination, itemsTransferred: 0, itemsTotal: 12, JobStatusChoices.JobHistoryErrorJobFailed);

            integrationPoint.JobHistory = SourceWorkspace.JobHistory.Select(x => x.ArtifactId).ToArray();
            SourceWorkspace.IntegrationPoints.Add(integrationPoint);
        }

        private string SetupDestinationWorkspace()
        {
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
            return $"Workspace - {destinationWorkspaceArtifactId}";
        }

        private void SetupNewJobHistoryTestData(IntegrationPointTest integrationPoint, string destinationName,
            int itemsTransferred, int itemsTotal, ChoiceRef status)
        {
            JobHistoryTest jobHistoryTest = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = itemsTransferred,
                TotalItems = itemsTotal,
                JobStatus = status,
                Overwrite = OverwriteModeNames.AppendOnlyModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(jobHistoryTest);
        }
    }
}
