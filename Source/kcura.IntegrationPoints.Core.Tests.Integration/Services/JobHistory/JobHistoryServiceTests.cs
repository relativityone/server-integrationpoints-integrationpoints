using System;
using System.Drawing;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.QueryOptions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryServiceTests : RelativityProviderTemplate
	{
		private JobHistoryService _sut;
		private Data.JobHistory _jobHistory;
		private Guid batchInstance;

		public JobHistoryServiceTests() 
			: base(nameof(JobHistoryServiceTests), targetWorkspaceName: null)
		{
		}

		public override void TestSetup()
		{
			ICaseServiceContext caseServiceContext = Container.Resolve<ICaseServiceContext>();
			IFederatedInstanceManager federatedInstanceManager = Container.Resolve<IFederatedInstanceManager>();
			IWorkspaceManager workspaceManager = Container.Resolve<IWorkspaceManager>();
			IHelper helper = Container.Resolve<IHelper>();
			IIntegrationPointSerializer serializer = Container.Resolve<IIntegrationPointSerializer>();
			IProviderTypeService providerTypeService = Container.Resolve<IProviderTypeService>();
			IMessageService messageService = Container.Resolve<IMessageService>();

			_sut = new JobHistoryService(
				caseServiceContext,
				federatedInstanceManager,
				workspaceManager,
				helper,
				serializer,
				providerTypeService,
				messageService
			);

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"{nameof(JobHistoryServiceTests)}{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
					.ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			batchInstance = Guid.NewGuid();
			_jobHistory = CreateJobHistoryOnIntegrationPoint(
				integrationPointCreated.ArtifactID,
				batchInstance,
				Data.JobTypeChoices.JobHistoryRun,
				Data.JobStatusChoices.JobHistoryCompletedWithErrors,
				jobEnded: true);
		}

		[Test]
		public void GetRdo_ShouldReturnLimitedRdoBasedOnTheQueryOptionsFields()
		{
			//arrange
			IQueryOptions queryOptions = Substitute.For<IQueryOptions>();
			queryOptions.Fields.Returns(new[]
			{
				"Name",
				"Job ID"
			});

			//act
			Data.JobHistory result = _sut.GetRdo(batchInstance, queryOptions);

			//assert
			result.ArtifactId.Should().Be(_jobHistory.ArtifactId);
			result.Name.Should().Be(_jobHistory.Name);
			result.JobID.Should().Be(_jobHistory.JobID);

			result.Documents.Should().BeNull();
			result.IntegrationPoint.Should().BeNull();
			result.JobStatus.Should().BeNull();
			result.ItemsTransferred.Should().BeNull();
			result.ItemsWithErrors.Should().BeNull();
			result.StartTimeUTC.Should().BeNull();
			result.EndTimeUTC.Should().BeNull();
			result.BatchInstance.Should().BeNull();
			result.DestinationWorkspace.Should().BeNull();
			result.TotalItems.Should().BeNull();
			result.DestinationWorkspaceInformation.Should().BeNull();
			result.DestinationInstance.Should().BeNull();
			result.FilesSize.Should().BeNull();
			result.Overwrite.Should().BeNull();
		}

		[Test]
		public void UpdateRdo_ShouldUpdateOnlyLimitedRdoBasedOnTheQueryOptionsFields()
		{
			//arrange
			IQueryOptions queryOptions = Substitute.For<IQueryOptions>();
			queryOptions.Fields.Returns(new[]
			{
				"Name",
				"Items Transferred"
			});
			var jobHistoryToUpdate = new Data.JobHistory
			{
				ArtifactId = _jobHistory.ArtifactId,
				Name = "New Name 1234",
				ItemsTransferred = 103
			};

			//act
			_sut.UpdateRdo(jobHistoryToUpdate, queryOptions);
			Data.JobHistory updatedJobHistory = _sut.GetRdo(batchInstance);

			//assert
			updatedJobHistory.ArtifactId.Should().Be(jobHistoryToUpdate.ArtifactId);
			updatedJobHistory.Name.Should().Be(jobHistoryToUpdate.Name);
			updatedJobHistory.ItemsTransferred.Should().Be(jobHistoryToUpdate.ItemsTransferred);

			updatedJobHistory.JobID.Should().Be(jobHistoryToUpdate.JobID);
			updatedJobHistory.Documents.Should().BeEquivalentTo(jobHistoryToUpdate.Documents);
			updatedJobHistory.IntegrationPoint.Should().BeEquivalentTo(jobHistoryToUpdate.IntegrationPoint);
			updatedJobHistory.JobStatus.Should().Be(jobHistoryToUpdate.JobStatus);
			updatedJobHistory.ItemsWithErrors.Should().Be(jobHistoryToUpdate.ItemsWithErrors);
			updatedJobHistory.StartTimeUTC.Should().Be(jobHistoryToUpdate.StartTimeUTC);
			updatedJobHistory.EndTimeUTC.Should().Be(jobHistoryToUpdate.EndTimeUTC);
			updatedJobHistory.BatchInstance.Should().Be(jobHistoryToUpdate.BatchInstance);
			updatedJobHistory.DestinationWorkspace.Should().Be(jobHistoryToUpdate.DestinationInstance);
			updatedJobHistory.TotalItems.Should().Be(jobHistoryToUpdate.TotalItems);
			updatedJobHistory.DestinationWorkspaceInformation.Should().BeEquivalentTo(jobHistoryToUpdate.DestinationWorkspaceInformation);
			updatedJobHistory.DestinationInstance.Should().Be(jobHistoryToUpdate.DestinationInstance);
			updatedJobHistory.FilesSize.Should().Be(jobHistoryToUpdate.FilesSize);
			updatedJobHistory.Overwrite.Should().Be(jobHistoryToUpdate.Overwrite);
		}
	}
}
