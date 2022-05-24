using FluentAssertions;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Constants;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class JobHistoryManagerTests : TestsBase
    {
        private IJobHistoryManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IJobHistoryManager>();
        }

        [IdentifiedTest("21FECB87-6307-43E9-900A-9119C94380DC")]
        public async Task GetJobHistoryAsync_ShouldReturnCorrectValues()
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 0,
                PageSize = 5
            };
            int expectedAvailableItems = SourceWorkspace.JobHistory.Count();
            int expectedTransferredItems = SourceWorkspace.JobHistory.Sum(x => x.ItemsTransferred) ?? 0;

            //Act
            JobHistorySummaryModel result = await _sut.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.TotalAvailable.Should().Be(expectedAvailableItems);
            result.TotalDocumentsPushed.Should().Be(expectedTransferredItems);
            result.Data.Length.Should().Be(expectedAvailableItems);
        }

        [IdentifiedTest("8C86BD4C-0BFD-4B01-A36E-5F3625EEF113")]
        public async Task GetJobHistoryAsync_ShouldReturnCorrectData_WhenTooLargeVolumeRequested()
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 15,
                PageSize = 15
            };

            //Act
            JobHistorySummaryModel result = await _sut.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [IdentifiedTest("90EC58F4-B25D-4CD6-8000-013D95A0DEDB")]
        public async Task GetJobHistoryAsync_ShouldReturnCorrectData_WhenPageAndPageSizeLimited()
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 1,
                PageSize = 2
            };
            int expectedIndexOfFirstReturnedDataSet = request.Page * request.PageSize;
            int expectedNumberOfReturnedDataSets = request.PageSize;

            List<JobHistoryTest> sourceData = SourceWorkspace.JobHistory.Skip(expectedIndexOfFirstReturnedDataSet).Take(expectedNumberOfReturnedDataSets).ToList();

            //Act
            JobHistorySummaryModel result = await _sut.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Data.Length.Should().Be(expectedNumberOfReturnedDataSets);

            for (int i = 0; i < result.Data.Length; i++)
            {
                result.Data[i].ItemsTransferred.Should().Be(sourceData[i].ItemsTransferred);
            }
        }

        [IdentifiedTestCase("878D128D-9868-4F3B-B66C-7E8E1E9BA1F6", nameof(JobHistoryModel.ItemsTransferred))]
        [IdentifiedTestCase("BB5B01D7-7CAC-4E82-9DC7-4EBB5B7CB2CA", nameof(JobHistoryModel.EndTimeUTC))]
        [IdentifiedTestCase("C803015A-5C75-4C08-AC25-FF4174D71FD4", nameof(JobHistoryModel.Overwrite))]
        public async Task GetJobHistoryAsync_ShouldReturnCorrectlySortedResult_WhenSortDescending(string property)
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 0,
                PageSize = 5,
                SortColumnName = property,
                SortDescending = true
            };

            //Act
            JobHistorySummaryModel result = await _sut.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            Assert.That(result.Data, Is.Ordered.Descending.By(property));
        }

        [IdentifiedTestCase("15961E3F-2827-41BE-A34B-B0442237536D", nameof(JobHistoryModel.ItemsTransferred))]
        [IdentifiedTestCase("E5B94AF6-5BB7-48AD-AB88-7F4E4EC81DC9", nameof(JobHistoryModel.EndTimeUTC))]
        [IdentifiedTestCase("8E13A8AF-03FD-4607-A95D-0B0E9F3F64D8", nameof(JobHistoryModel.Overwrite))]
        public async Task GetJobHistoryAsync_ShouldReturnCorrectlySortedResult_WhenSortAscending(string property)
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 0,
                PageSize = 5,
                SortColumnName = property,
                SortDescending = false
            };

            //Act
            JobHistorySummaryModel result = await _sut.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            Assert.That(result.Data, Is.Ordered.By(property));
        }

        private void AddTestData()
        {
            WorkspaceTest destinationWorkspace = SetupDestinationWorkspace(out string destinationName);
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);

            SetupJobHistoryTestData(integrationPoint, destinationName);          
        }

        private WorkspaceTest SetupDestinationWorkspace(out string destinationName)
        {
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            destinationName = $"Workspace - {destinationWorkspaceArtifactId}";
            return FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
        }

        private void SetupJobHistoryTestData(IntegrationPointTest integrationPoint, string destinationName)
        {
            SourceWorkspace.Helpers.JobHistoryHelper.CreateCustomJobHistory(integrationPoint, destinationName,
               DateTime.Now, JobStatusChoices.JobHistoryCompleted, itemsTransferred: 17, totalItems: 17, overwrite: OverwriteModeNames.AppendOnlyModeName);
            SourceWorkspace.Helpers.JobHistoryHelper.CreateCustomJobHistory(integrationPoint, destinationName,
                 DateTime.Now.AddMinutes(30), JobStatusChoices.JobHistoryCompletedWithErrors, itemsTransferred: 6, totalItems: 8, overwrite: OverwriteModeNames.AppendOverlayModeName);
            SourceWorkspace.Helpers.JobHistoryHelper.CreateCustomJobHistory(integrationPoint, destinationName,
                DateTime.Now.AddHours(2), JobStatusChoices.JobHistoryCompleted, itemsTransferred: 10, totalItems: 10, overwrite: OverwriteModeNames.OverlayOnlyModeName);
            SourceWorkspace.Helpers.JobHistoryHelper.CreateCustomJobHistory(integrationPoint, destinationName,
                DateTime.Now.AddMinutes(10), JobStatusChoices.JobHistoryCompletedWithErrors, itemsTransferred: 2, totalItems: 5, overwrite: OverwriteModeNames.AppendOnlyModeName);
            SourceWorkspace.Helpers.JobHistoryHelper.CreateCustomJobHistory(integrationPoint, destinationName,
                DateTime.Now.AddSeconds(15), JobStatusChoices.JobHistoryErrorJobFailed, itemsTransferred: 0, totalItems: 12, overwrite: OverwriteModeNames.OverlayOnlyModeName);
        }
    }
}
