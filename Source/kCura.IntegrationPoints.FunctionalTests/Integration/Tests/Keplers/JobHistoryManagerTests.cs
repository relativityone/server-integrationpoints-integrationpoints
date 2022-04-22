using FluentAssertions;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        }

        [IdentifiedTest("21FECB87-6307-43E9-900A-9119C94380DC")]
        public async Task ItShouldReturnJobHistoryWithCorrectValues()
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
            AddTestData();
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
            JobHistorySummaryModel result = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            result.Data.Length.Should().Be(expectedNumberOfReturnedDataSets);

            for (int i = 0; i < result.Data.Length; i++)
            {
                result.Data[i].ItemsTransferred.Should().Be(sourceData[i].ItemsTransferred);
            }
        }

        [IdentifiedTestCase("878D128D-9868-4F3B-B66C-7E8E1E9BA1F6", nameof(JobHistoryModel.ItemsTransferred), true)]
        [IdentifiedTestCase("15961E3F-2827-41BE-A34B-B0442237536D", nameof(JobHistoryModel.ItemsTransferred), false)]
        [IdentifiedTestCase("BB5B01D7-7CAC-4E82-9DC7-4EBB5B7CB2CA", nameof(JobHistoryModel.EndTimeUTC), true)]
        [IdentifiedTestCase("E5B94AF6-5BB7-48AD-AB88-7F4E4EC81DC9", nameof(JobHistoryModel.EndTimeUTC), false)]
        [IdentifiedTestCase("C803015A-5C75-4C08-AC25-FF4174D71FD4", nameof(JobHistoryModel.Overwrite), true)]
        [IdentifiedTestCase("8E13A8AF-03FD-4607-A95D-0B0E9F3F64D8", nameof(JobHistoryModel.Overwrite), false)]
        public async Task JobHistoryResultShouldBeCorrectlySorted(string property, bool sortDescending)
        {
            //Arrange
            AddTestData();
            JobHistoryRequest request = new JobHistoryRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                Page = 0,
                PageSize = 5,
                SortColumnName = property,
                SortDescending = sortDescending
            };
           
            //Act
            JobHistorySummaryModel result = await _manager.GetJobHistoryAsync(request).ConfigureAwait(false);

            //Assert
            if (sortDescending)
            {
                Assert.That(result.Data, Is.Ordered.Descending.By(property));
            }
            else
            {
                Assert.That(result.Data, Is.Ordered.By(property));
            }                
        }

        private void AddTestData()
        {
            string destination = SetupDestinationWorkspace();
            IntegrationPointTest integrationPoint = new IntegrationPointTest
            {
                SourceProvider = SourceWorkspace.SourceProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY).ArtifactId,
                DestinationProvider = SourceWorkspace.DestinationProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY).ArtifactId
            };

            SetupJobHistoryTestData(integrationPoint, destination);

            integrationPoint.JobHistory = SourceWorkspace.JobHistory.Select(x => x.ArtifactId).ToArray();
            SourceWorkspace.IntegrationPoints.Add(integrationPoint);
        }

        private string SetupDestinationWorkspace()
        {
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
            return $"Workspace - {destinationWorkspaceArtifactId}";
        }

        private void SetupJobHistoryTestData(IntegrationPointTest integrationPoint, string destinationName)
        {
            JobHistoryTest case1 = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = 17,
                TotalItems = 17,
                EndTimeUTC = DateTime.Now,
                JobStatus = JobStatusChoices.JobHistoryCompleted,
                Overwrite = OverwriteModeNames.AppendOnlyModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(case1);

            JobHistoryTest case2 = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = 6,
                TotalItems = 8,
                EndTimeUTC = DateTime.Now.AddMinutes(30),
                JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
                Overwrite = OverwriteModeNames.AppendOverlayModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(case2);

            JobHistoryTest case3 = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = 10,
                TotalItems = 10,
                EndTimeUTC = DateTime.Now.AddHours(2),
                JobStatus = JobStatusChoices.JobHistoryCompleted,
                Overwrite = OverwriteModeNames.OverlayOnlyModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(case3);

            JobHistoryTest case4 = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = 2,
                TotalItems = 5,
                EndTimeUTC = DateTime.Now.AddMinutes(10),
                JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
                Overwrite = OverwriteModeNames.AppendOnlyModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(case4);

            JobHistoryTest case5 = new JobHistoryTest
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = 0,
                TotalItems = 12,
                EndTimeUTC = DateTime.Now.AddSeconds(15),
                JobStatus = JobStatusChoices.JobHistoryErrorJobFailed,
                Overwrite = OverwriteModeNames.OverlayOnlyModeName,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            SourceWorkspace.JobHistory.Add(case5);
        }
    }
}
