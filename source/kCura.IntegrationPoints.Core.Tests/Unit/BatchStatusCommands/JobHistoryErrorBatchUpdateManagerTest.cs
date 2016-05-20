using System;
using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class JobHistoryErrorBatchUpdateManagerTest
	{
		private ITempDocTableHelper _tempTableHelper;
		private ITempDocumentTableFactory _tempTableFactory;
		private IRepositoryFactory _repositoryFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private IBatchStatus _testInstance;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IObjectTypeRepository _objectTypeRepository;
		private IArtifactGuidRepository _artifactGuidRepository;
		private ClaimsPrincipal _claimsPrincipal;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private const int _jobHistoryErrorTypeId = 6873784;
		private const int _errorStatusExpiredChoiceArtifactId = 798523;
		private const int _errorStatusInProgressChoiceArtifactId = 651368;
		private const int _errorStatusRetriedChoiceArtifactId = 035463;
		private const int _sourceWorkspaceId = 1357475;
		private const int _submittedBy = 1385796;
		private const int _savedSearchArtifactId = 1668735;
		private readonly Guid _jobHistoryErrorGuid = new Guid("17e7912d-4f57-4890-9a37-abc2b8a37bdb");
		private const string _jobErrorOnStartPrefix = "IntegrationPoint_Relativity_JHE_Job1";
		private const string _jobErrorOnCompletePrefix = "IntegrationPoint_Relativity_JHE_Job2";
		private const string _itemErrorOnStartPrefix = "IntegrationPoint_Relativity_JHE_Item1";
		private const string _itemErrorOnCompletePrefix = "IntegrationPoint_Relativity_JHE_Item2";
		private const string _uniqueJobId = "aceVentura";
		private const string _noResultsForObjectType = "Unable to retrieve Artifact Type Id for JobHistoryError object type.";
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_tempTableHelper = Substitute.For<ITempDocTableHelper>();
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_tempTableFactory = Substitute.For<ITempDocumentTableFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_updateStatusType = Substitute.For<JobHistoryErrorDTO.UpdateStatusType>();

			_tempTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceId).Returns(_tempTableHelper);
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);
			_repositoryFactory.GetObjectTypeRepository(_sourceWorkspaceId).Returns(_objectTypeRepository);
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(_jobHistoryErrorGuid).Returns(_jobHistoryErrorTypeId);

			_testInstance = new JobHistoryErrorBatchUpdateManager(_tempTableFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory,
				_sourceWorkspaceId, _uniqueJobId, _submittedBy, _updateStatusType, _savedSearchArtifactId);

			_repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceId).Returns(_artifactGuidRepository);
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids)
				.Returns(new Dictionary<Guid, int>() {{ ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids[0], _errorStatusExpiredChoiceArtifactId } });
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorInProgress.ArtifactGuids)
				.Returns(new Dictionary<Guid, int>() { { ErrorStatusChoices.JobHistoryErrorInProgress.ArtifactGuids[0], _errorStatusInProgressChoiceArtifactId } });
			_artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorRetried.ArtifactGuids)
				.Returns(new Dictionary<Guid, int>() { { ErrorStatusChoices.JobHistoryErrorRetried.ArtifactGuids[0], _errorStatusRetriedChoiceArtifactId } });

			_tempTableFactory.Received().GetDocTableHelper(_uniqueJobId, _sourceWorkspaceId);
			_onBehalfOfUserClaimsPrincipalFactory.Received().CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		public void OnJobStart_RunNow_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), 
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobStart_RunNow_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusExpiredChoiceArtifactId, _jobErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnStartPrefix);
		}

		[Test]
		public void OnJobStart_RunNow_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusExpiredChoiceArtifactId, _jobErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnStartPrefix);
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusExpiredChoiceArtifactId, _itemErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_itemErrorOnStartPrefix);
		}

		[Test]
		public void OnJobStart_RunNow_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobStart(_job);

			//Assert
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusExpiredChoiceArtifactId, _itemErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_itemErrorOnStartPrefix);
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
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusInProgressChoiceArtifactId, _jobErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnStartPrefix);
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusInProgressChoiceArtifactId, _jobErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnStartPrefix);
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusExpiredChoiceArtifactId, _itemErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_itemErrorOnStartPrefix);
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusInProgressChoiceArtifactId, _itemErrorOnStartPrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_itemErrorOnStartPrefix);
		}

		[Test]
		public void OnJobComplete_RunNow_NoErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_JobError()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_JobAndItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
		}

		[Test]
		public void OnJobComplete_RunNow_ItemErrors()
		{
			//Arrange
			_updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			_updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

			//Act
			_testInstance.OnJobComplete(_job);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(),
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
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
				Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusRetriedChoiceArtifactId, _jobErrorOnCompletePrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnCompletePrefix);
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusRetriedChoiceArtifactId, _jobErrorOnCompletePrefix + "_" + _uniqueJobId);
			_tempTableHelper.Received().DeleteTable(_jobErrorOnCompletePrefix);
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
			_jobHistoryErrorRepository.Received().UpdateErrorStatuses(_claimsPrincipal, Arg.Any<int>(), _jobHistoryErrorTypeId, _sourceWorkspaceId,
				_errorStatusRetriedChoiceArtifactId, _itemErrorOnCompletePrefix + "_" + _uniqueJobId);
			_jobHistoryErrorRepository.Received().DeleteItemLevelErrorsSavedSearch(_sourceWorkspaceId, _savedSearchArtifactId, 0);
			_tempTableHelper.Received().DeleteTable(_itemErrorOnCompletePrefix);
		}

		[Test]
		public void JobHistoryErrorBatchUpdateManagerInitializationFailure_NoMatchingObjectType()
		{
			//Arrange
			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(_jobHistoryErrorGuid).Returns(new int?());

			//Act
			Exception ex = Assert.Throws<Exception>(() => new JobHistoryErrorBatchUpdateManager(_tempTableFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory,
				_sourceWorkspaceId, _uniqueJobId, _submittedBy, _updateStatusType, _savedSearchArtifactId));

			//Assert
			Assert.AreEqual(_noResultsForObjectType, ex.Message);
		}
	}
}