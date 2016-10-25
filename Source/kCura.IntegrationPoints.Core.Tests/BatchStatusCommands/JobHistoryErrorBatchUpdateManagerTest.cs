using System;
using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
	[TestFixture]
	public class JobHistoryErrorBatchUpdateManagerTest
	{
		private IScratchTableRepository _scratchTableRepository;
		private IRepositoryFactory _repositoryFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private IBatchStatus _testInstance;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IObjectTypeRepository _objectTypeRepository;
		private IArtifactGuidRepository _artifactGuidRepository;
		private readonly ClaimsPrincipal _claimsPrincipal = null;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private const int _jobHistoryErrorTypeId = 6873784;
		private const int _errorStatusExpiredChoiceArtifactId = 798523;
		private const int _errorStatusInProgressChoiceArtifactId = 651368;
		private const int _errorStatusRetriedChoiceArtifactId = 035463;
		private const int _sourceWorkspaceId = 1357475;
		private const int _submittedBy = 1385796;
		private const int _savedSearchArtifactId = 1668735;
		private readonly Guid _jobHistoryErrorGuid = new Guid("17e7912d-4f57-4890-9a37-abc2b8a37bdb");
		private const string _noResultsForObjectType = "Unable to retrieve Artifact Type Id for JobHistoryError object type.";
		private readonly Job _job = null;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private IJobStopManager _jobStopManager;

		private const string _SCRATCHTABLE_ITEMSTART = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart";
		private const string _SCRATCHTABLE_ITEMCOMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
		private const string _SCRATCHTABLE_JOBSTART = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		private const string _SCRATCHTABLE_JOBCOMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";

		[SetUp]
		public void Setup()
		{
			_jobStopManager = Substitute.For<IJobStopManager>();
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_updateStatusType = Substitute.For<JobHistoryErrorDTO.UpdateStatusType>();
			_jobHistoryErrorManager = Substitute.For<IJobHistoryErrorManager>();
			
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);
			_repositoryFactory.GetObjectTypeRepository(_sourceWorkspaceId).Returns(_objectTypeRepository);
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(_jobHistoryErrorGuid).Returns(_jobHistoryErrorTypeId);

			_jobHistoryErrorManager.JobHistoryErrorItemStart.Returns(Substitute.For<IScratchTableRepository>());
			_jobHistoryErrorManager.JobHistoryErrorItemComplete.Returns(Substitute.For<IScratchTableRepository>());
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Returns(Substitute.For<IScratchTableRepository>());
			_jobHistoryErrorManager.JobHistoryErrorJobComplete.Returns(Substitute.For<IScratchTableRepository>());

			_jobHistoryErrorManager.JobHistoryErrorItemStart.GetTempTableName().Returns(_SCRATCHTABLE_ITEMSTART);
			_jobHistoryErrorManager.JobHistoryErrorItemComplete.GetTempTableName().Returns(_SCRATCHTABLE_ITEMCOMPLETE);
			_jobHistoryErrorManager.JobHistoryErrorJobStart.GetTempTableName().Returns(_SCRATCHTABLE_JOBSTART);
			_jobHistoryErrorManager.JobHistoryErrorJobComplete.GetTempTableName().Returns(_SCRATCHTABLE_JOBCOMPLETE);

			_testInstance = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _jobStopManager,
				_sourceWorkspaceId, _submittedBy, _updateStatusType);

			_repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceId).Returns(_artifactGuidRepository);
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.Guids)
				.Returns(new Dictionary<Guid, int>() {{ ErrorStatusChoices.JobHistoryErrorExpired.Guids[0], _errorStatusExpiredChoiceArtifactId } });
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorInProgress.Guids)
				.Returns(new Dictionary<Guid, int>() { { ErrorStatusChoices.JobHistoryErrorInProgress.Guids[0], _errorStatusInProgressChoiceArtifactId } });
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorRetried.Guids)
				.Returns(new Dictionary<Guid, int>() { { ErrorStatusChoices.JobHistoryErrorRetried.Guids[0], _errorStatusRetriedChoiceArtifactId } });

			_onBehalfOfUserClaimsPrincipalFactory.Received().CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		public void OnJobStart_RunNow_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), 
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobStart_RunNow_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_JOBSTART);
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobStart_RunNow_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_JOBSTART);
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).Dispose();
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_ITEMSTART);
			_jobHistoryErrorManager.JobHistoryErrorItemStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobStart_RunNow_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_ITEMSTART);
			_jobHistoryErrorManager.JobHistoryErrorItemStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobStart_RetryErrors_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobStart_RetryErrors_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).CopyTempTable(_SCRATCHTABLE_JOBCOMPLETE);
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusInProgressChoiceArtifactId, _SCRATCHTABLE_JOBSTART);
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobStart_RetryErrors_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).CopyTempTable(_SCRATCHTABLE_JOBCOMPLETE);
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusInProgressChoiceArtifactId, _SCRATCHTABLE_JOBSTART);
			_jobHistoryErrorManager.JobHistoryErrorJobStart.Received(1).Dispose();
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_ITEMSTART);
			_jobHistoryErrorManager.JobHistoryErrorItemStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobStart_RetryErrors_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorManager.JobHistoryErrorItemStart.Received(1).CopyTempTable(_SCRATCHTABLE_ITEMCOMPLETE);
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusInProgressChoiceArtifactId, _SCRATCHTABLE_ITEMSTART);
			_jobHistoryErrorManager.JobHistoryErrorItemStart.Received(1).Dispose();
		}

		[Test]
		public void OnJobComplete_RunNow_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RetryErrors_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RetryErrors_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusRetriedChoiceArtifactId, _SCRATCHTABLE_JOBCOMPLETE);
			_jobHistoryErrorManager.JobHistoryErrorJobComplete.Received(1).Dispose();
		}

		[Test]
		public void OnJobComplete_RetryErrors_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusRetriedChoiceArtifactId, _SCRATCHTABLE_JOBCOMPLETE);
			_jobHistoryErrorManager.JobHistoryErrorJobComplete.Received(1).Dispose();
		}

		[Test]
		public void OnJobComplete_RetryErrors_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusRetriedChoiceArtifactId,_SCRATCHTABLE_ITEMCOMPLETE);
			_jobHistoryErrorManager.JobHistoryErrorItemComplete.Received(1).Dispose();
		}

		[Test]
		public void OnJobComplete_RetryErrors_StopRequested()
		{
			//Arrange
			_jobStopManager.IsStopRequested().Returns(true);

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.Received(1).UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId,
				_errorStatusExpiredChoiceArtifactId, _SCRATCHTABLE_ITEMCOMPLETE);
			_jobHistoryErrorManager.JobHistoryErrorItemComplete.Received(1).Dispose();
		}
	}
}