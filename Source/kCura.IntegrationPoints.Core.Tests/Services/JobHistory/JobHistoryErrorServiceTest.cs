using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryErrorServiceTest : TestBase
	{
		private Data.IntegrationPoint _integrationPoint;
		private Data.JobHistory _jobHistory;

		private ICaseServiceContext _caseServiceContext;
		private IHelper _helper;

		private JobHistoryErrorService _instance;
		private IJobStopManager _stopJobManager;

		[SetUp]
		public override void SetUp()
		{
			_integrationPoint = new Data.IntegrationPoint() {LogErrors = true};
			_jobHistory = new Data.JobHistory { ArtifactId = 111 };

			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_helper = Substitute.For<IHelper>();
			_stopJobManager = Substitute.For<IJobStopManager>();

			_instance = new JobHistoryErrorService(_caseServiceContext, _helper)
			{
				IntegrationPoint = _integrationPoint,
				JobHistory = _jobHistory,
				JobStopManager = _stopJobManager
			};
		}

		[Test]
		public void CommitErrors_HasJobHistory_CommitsJobHistoryErrors_ForDocumentLevelErrors()
		{
			// Arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier2", "Fake item error2.", "stack trace2");
			List<JobHistoryError> errors = new List<JobHistoryError>();
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			_instance.CommitErrors();

			// Assert
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			Assert.AreEqual(2, errors.Count);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, errors[0].ErrorType.Name);
			Assert.AreEqual("Fake item error.", errors[0].Error);
			Assert.AreEqual("stack trace", errors[0].StackTrace);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, errors[1].ErrorType.Name);
			Assert.AreEqual("Fake item error2.", errors[1].Error);
			Assert.AreEqual("stack trace2", errors[1].StackTrace);
			Assert.IsNotNull(_instance.IntegrationPoint.HasErrors);
			Assert.IsTrue(_instance.IntegrationPoint.HasErrors.Value);
		}


		[Test]
		public void AddError_CommitsJobHistoryErrors_ForJobLevelErrors()
		{
			// Arrange
			List<JobHistoryError> errors = new List<JobHistoryError>();
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", "stack trace");
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error2.", "stack trace2");

			// Assert
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(2).Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			Assert.AreEqual(2, errors.Count);
		}


		[Test]
		public void CommitErrors_HasJobHistory_NoErrorsToCommit()
		{
			// Arrange
			_instance.IntegrationPoint.HasErrors = true;

			// Act
			_instance.CommitErrors();

			// Assert
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.DidNotReceive().Create(Arg.Any<IEnumerable<JobHistoryError>>());
			Assert.IsNotNull(_instance.IntegrationPoint.HasErrors);
			Assert.IsFalse(_instance.IntegrationPoint.HasErrors.Value);
		}

		[Test]
		public void CommitErrors_FailsCommit_ThrowsException_ItemLevelError()
		{
			// Arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", null);
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Any<IEnumerable<JobHistoryError>>()).Throws(new Exception());
			_caseServiceContext.RsapiService.RelativityObjectManager.Update(Arg.Any<Data.IntegrationPoint>()).Returns(true);
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			Exception returnedException = Assert.Throws<Exception>(() => _instance.CommitErrors());

			// Assert
			_caseServiceContext.RsapiService.RelativityObjectManager.Received().Update(Arg.Any<Data.IntegrationPoint>());
			Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Item    Identifier: MyIdentifier    Error: Fake item error."));
		}

		[Test]
		public void CommitErrors_FailsCommit_ThrowsException_JobLevelError()
		{
			// Arrange
		_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Any<IEnumerable<JobHistoryError>>()).Throws(new Exception());
			_caseServiceContext.RsapiService.RelativityObjectManager.Update(Arg.Any<Data.IntegrationPoint>()).Returns(true);
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			//Adding job level error automatically commits errors
			Exception returnedException = Assert.Throws<Exception>(() => _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

			// Assert
			_caseServiceContext.RsapiService.RelativityObjectManager.Received().Update(Arg.Any<Data.IntegrationPoint>());
			Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Job    Error: Fake job error." + Environment.NewLine));
		}

		[Test]
		public void AddError_NoJobHistory_ThrowsException()
		{
			// Arrange
			_instance.JobHistory = null;

			// Act
			Exception returnedException = Assert.Throws<Exception>(() => _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

			// Assert
			_caseServiceContext.RsapiService.RelativityObjectManager.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.DidNotReceive().Create(Arg.Any<IEnumerable<JobHistoryError>>());
			Assert.That(returnedException.Message, Is.EqualTo("Type:Job Id:  Error:Fake job error."));
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void OnRowError_DoNotAddErrorWhenStopped()
		{
			// ARRANGE
			const string identifier = "identifier";
			Reporter reporter = new Reporter();
			_stopJobManager.IsStopRequested().Returns(true);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			reporter.RaiseDocumentError(identifier, identifier);

			// ASSERT
			Assert.AreEqual(0, _instance.PendingErrorCount);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void OnRowError_AddErrorWhenRunning()
		{
			// ARRANGE
			const string identifier = "identifier";
			Reporter reporter = new Reporter();
			_stopJobManager.IsStopRequested().Returns(false);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			reporter.RaiseDocumentError(identifier, identifier);

			// ASSERT
			Assert.AreEqual(1, _instance.PendingErrorCount);
		}

		[Test]
		public void AddError_CommitErrorsByBatch()
		{
			// ARRANGE
			Exception exception = new Exception();
			Reporter reporter = new Reporter();
			_stopJobManager.IsStopRequested().Returns(true);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			for (int i = 0; i < JobHistoryErrorService.ERROR_BATCH_SIZE; i ++)
			{
				_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, exception);
			}

			// ASSERT 
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Is<IEnumerable<JobHistoryError>>(errors => errors.Count() == JobHistoryErrorService.ERROR_BATCH_SIZE));
		}

		[TestCase(true)]
		[TestCase(false)]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void OnJobError_AlwaysAddError(bool isStopped)
		{
			// ARRANGE
			Reporter reporter = new Reporter();
			Exception exception = new Exception();
			_stopJobManager.IsStopRequested().Returns(isStopped);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			reporter.RaiseOnJobError(exception);

			// ASSERT 
			Assert.IsTrue(_instance.JobLevelErrorOccurred);
		}

		[Test]
		public void CommitErrors_SetHasErrorToFalseWhenStopAndNoErrorOccured()
		{
			// arrange
			_stopJobManager.IsStopRequested().Returns(true);

			// act
			_instance.CommitErrors();

			// assert
			Assert.IsFalse(_integrationPoint.HasErrors);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);

		}

		[Test]
		public void CommitErrors_SetHasErrorToFalseWhenRunningAndNoErrorOccured()
		{
			// arrange
			_stopJobManager.IsStopRequested().Returns(false);

			// act
			_instance.CommitErrors();

			// assert
			Assert.IsFalse(_integrationPoint.HasErrors);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);
		}

		[Test]
		public void CommitErrors_SetHasErrorToFalseWhenStopAndErrorsOccured()
		{
			// arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, new Exception());
			_stopJobManager.IsStopRequested().Returns(true);

			// act
			_instance.CommitErrors();

			// assert
			Assert.IsFalse(_integrationPoint.HasErrors);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);
		}

		[Test]
		public void CommitErrors_SetHasErrorToTrueWhenRunningAndErrorsOccured()
		{
			// arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, new Exception());
			_stopJobManager.IsStopRequested().Returns(false);

			// act
			_instance.CommitErrors();

			// assert
			Assert.IsTrue(_integrationPoint.HasErrors);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);
		}

		[Test]
		public void CommitErrors_SuppressErrorOnUpdateHasErrorField()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.RelativityObjectManager.Update(Arg.Any<Data.IntegrationPoint>()).Throws(new Exception());

			// ACT & ASSERT
			Assert.DoesNotThrow(() => _instance.CommitErrors());
		}

		private class Reporter : IBatchReporter
		{
			public event BatchCompleted OnBatchComplete { add { } remove { } }
			public event BatchSubmitted OnBatchSubmit { add { } remove { } }
			public event BatchCreated OnBatchCreate { add { } remove { } }
			public event StatusUpdate OnStatusUpdate { add { } remove { } }
			public event JobError OnJobError;
			public event RowError OnDocumentError;
			public event StatisticsUpdate OnStatisticsUpdate;

			public void RaiseDocumentError(string identifier, string msg)
			{
				if (OnDocumentError != null)
				{
					OnDocumentError(identifier, msg);
				}
			}

			public void RaiseOnJobError(Exception ex)
			{
				if (OnJobError != null)
				{
					OnJobError(ex);
				}
			}

			private void Fake()
			{
				OnStatisticsUpdate?.Invoke(0, 0);
			}
		}
	}
}