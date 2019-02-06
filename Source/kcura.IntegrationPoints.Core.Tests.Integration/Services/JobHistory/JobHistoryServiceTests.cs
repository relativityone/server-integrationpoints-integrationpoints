using System;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryServiceTests : RelativityProviderTemplate
	{
		private JobHistoryService _sut;
		private int _jobHistoryArtifactId;
		private Guid _batchInstance;

		public JobHistoryServiceTests() 
			: base(nameof(JobHistoryServiceTests), targetWorkspaceName: null)
		{
		}

		public override void TestSetup()
		{
			IRelativityObjectManager relativityObjectManager = Container.Resolve<IRelativityObjectManager>();
			IFederatedInstanceManager federatedInstanceManager = Container.Resolve<IFederatedInstanceManager>();
			IWorkspaceManager workspaceManager = Container.Resolve<IWorkspaceManager>();
			IHelper helper = Container.Resolve<IHelper>();
			IIntegrationPointSerializer serializer = Container.Resolve<IIntegrationPointSerializer>();
			IProviderTypeService providerTypeService = Container.Resolve<IProviderTypeService>();
			IMessageService messageService = Container.Resolve<IMessageService>();

			_sut = new JobHistoryService(
				relativityObjectManager,
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
			_batchInstance = Guid.NewGuid();

			Data.JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(
				integrationPointCreated.ArtifactID,
				_batchInstance,
				Data.JobTypeChoices.JobHistoryRun,
				Data.JobStatusChoices.JobHistoryCompletedWithErrors,
				jobEnded: true);

			_jobHistoryArtifactId = jobHistory.ArtifactId;
		}

		[Test]
		public void GetRdoWithoutDocuments_ShouldReturnRdoWithoutDocumentsField()
		{
			//arrange
			Data.JobHistory jobHistoryWithAllFieldsFetched = _sut.GetRdo(_batchInstance);

			//act
			Data.JobHistory result = _sut.GetRdoWithoutDocuments(_batchInstance);

			//assert
			Action getDocuments = () =>
			{
				int[] docs = result.Documents;
			};
			getDocuments.ShouldThrow<FieldNotFoundException>();

			Action getOtherFields = () =>
			{
				int artifactId = result.ArtifactId;
				string name = result.Name;
				string jobId = result.JobID;
				int[] integrationPoint = result.IntegrationPoint;
				Choice jobStatus = result.JobStatus;
				int? itemsTransferred = result.ItemsTransferred;
				int? itemsWithErrors = result.ItemsWithErrors;
				DateTime? startTimeUtc = result.StartTimeUTC;
				DateTime? endTimeUtc = result.EndTimeUTC;
				string bInstance = result.BatchInstance;
				string destinationWorkspace = result.DestinationWorkspace;
				long? totalItems = result.TotalItems;
				int[] destinationWorkspaceInfo = result.DestinationWorkspaceInformation;
				string destinationInstance = result.DestinationInstance;
				string fileSize = result.FilesSize;
				string overwrite = result.Overwrite;
			};
			getOtherFields.ShouldNotThrow<FieldNotFoundException>();

			result.ArtifactId.Should().Be(jobHistoryWithAllFieldsFetched.ArtifactId);
			result.Name.Should().Be(jobHistoryWithAllFieldsFetched.Name);
			result.JobID.Should().Be(jobHistoryWithAllFieldsFetched.JobID);
			result.IntegrationPoint.ShouldBeEquivalentTo(jobHistoryWithAllFieldsFetched.IntegrationPoint);
			result.JobStatus.ArtifactID.Should().Be(jobHistoryWithAllFieldsFetched.JobStatus.ArtifactID);
			result.JobStatus.Name.Should().Be(jobHistoryWithAllFieldsFetched.JobStatus.Name);
			result.ItemsTransferred.Should().Be(jobHistoryWithAllFieldsFetched.ItemsTransferred);
			result.ItemsWithErrors.Should().Be(jobHistoryWithAllFieldsFetched.ItemsWithErrors);
			result.StartTimeUTC.Should().Be(jobHistoryWithAllFieldsFetched.StartTimeUTC);
			result.EndTimeUTC.Should().Be(jobHistoryWithAllFieldsFetched.EndTimeUTC);
			result.BatchInstance.Should().Be(jobHistoryWithAllFieldsFetched.BatchInstance);
			result.DestinationWorkspace.Should().Be(jobHistoryWithAllFieldsFetched.DestinationWorkspace);
			result.TotalItems.Should().Be(jobHistoryWithAllFieldsFetched.TotalItems);
			result.DestinationWorkspaceInformation.ShouldBeEquivalentTo(jobHistoryWithAllFieldsFetched.DestinationWorkspaceInformation);
			result.DestinationInstance.Should().Be(jobHistoryWithAllFieldsFetched.DestinationInstance);
			result.FilesSize.Should().Be(jobHistoryWithAllFieldsFetched.FilesSize);
			result.Overwrite.Should().Be(jobHistoryWithAllFieldsFetched.Overwrite);
		}

		[Test]
		public void UpdateRdoWithoutDocuments_ShouldUpdateRdoWithoutDocumentsField()
		{
			//arrange
			Data.JobHistory oldJobHistory = _sut.GetRdo(_batchInstance);

			var jobHistoryToUpdate = new Data.JobHistory
			{
				ArtifactId = _jobHistoryArtifactId,
				Name = "New Name 1234",
				Documents = new []{ 1, 2, 3, 4}
			};

			//act
			_sut.UpdateRdoWithoutDocuments(jobHistoryToUpdate);

			//assert
			Data.JobHistory updatedJobHistory = _sut.GetRdo(_batchInstance);

			updatedJobHistory.Documents.Should().BeEquivalentTo(oldJobHistory.Documents);

			updatedJobHistory.ArtifactId.Should().Be(jobHistoryToUpdate.ArtifactId);
			updatedJobHistory.Name.Should().Be(jobHistoryToUpdate.Name);
			updatedJobHistory.ItemsTransferred.Should().Be(oldJobHistory.ItemsTransferred);
			updatedJobHistory.JobID.Should().Be(oldJobHistory.JobID);
			updatedJobHistory.IntegrationPoint.Should().BeEquivalentTo(oldJobHistory.IntegrationPoint);
			updatedJobHistory.JobStatus.ArtifactID.Should().Be(oldJobHistory.JobStatus.ArtifactID);
			updatedJobHistory.JobStatus.Name.Should().Be(oldJobHistory.JobStatus.Name);
			updatedJobHistory.ItemsWithErrors.Should().Be(oldJobHistory.ItemsWithErrors);
			updatedJobHistory.StartTimeUTC.Should().Be(oldJobHistory.StartTimeUTC);
			updatedJobHistory.EndTimeUTC.Should().Be(oldJobHistory.EndTimeUTC);
			updatedJobHistory.BatchInstance.Should().Be(oldJobHistory.BatchInstance);
			updatedJobHistory.DestinationWorkspace.Should().Be(oldJobHistory.DestinationWorkspace);
			updatedJobHistory.TotalItems.Should().Be(oldJobHistory.TotalItems);
			updatedJobHistory.DestinationWorkspaceInformation.Should().BeEquivalentTo(oldJobHistory.DestinationWorkspaceInformation);
			updatedJobHistory.DestinationInstance.Should().Be(oldJobHistory.DestinationInstance);
			updatedJobHistory.FilesSize.Should().Be(oldJobHistory.FilesSize);
			updatedJobHistory.Overwrite.Should().Be(oldJobHistory.Overwrite);
		}
	}
}
