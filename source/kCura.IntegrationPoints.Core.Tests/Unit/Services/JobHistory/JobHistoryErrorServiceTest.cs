﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryErrorServiceTest
	{
		private Data.IntegrationPoint _integrationPoint;
		private Data.JobHistory _jobHistory;

		private ICaseServiceContext _caseServiceContext;

		private JobHistoryErrorService _instance;
		private IJobStopManager _stopJobManager;

		[SetUp]
		public void SetUp()
		{
			_integrationPoint = new Data.IntegrationPoint() {LogErrors = true};
			_jobHistory = new Data.JobHistory { ArtifactId = 111 };

			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_stopJobManager = Substitute.For<IJobStopManager>();

			_instance = new JobHistoryErrorService(_caseServiceContext)
			{
				IntegrationPoint = _integrationPoint,
				JobHistory = _jobHistory,
				JobStopManager = _stopJobManager
			};
		}

		[Test]
		public void CommitErrors_HasJobHistory_CommitsJobHistoryErrors()
		{
			// Arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null);
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");
			List<JobHistoryError> errors = new List<JobHistoryError>();
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			_instance.CommitErrors();

			// Assert
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			Assert.AreEqual(2, errors.Count);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorJob.Name, errors[0].ErrorType.Name);
			Assert.AreEqual("Fake job error.", errors[0].Error);
			Assert.AreEqual(null, errors[0].StackTrace);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, errors[1].ErrorType.Name);
			Assert.AreEqual("Fake item error.", errors[1].Error);
			Assert.AreEqual("stack trace", errors[1].StackTrace);
			Assert.IsNotNull(_instance.IntegrationPoint.HasErrors);
			Assert.IsTrue(_instance.IntegrationPoint.HasErrors.Value);
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
		public void CommitErrors_FailsCommit_ThrowsException()
		{
			// Arrange
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null);
			_instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", null);
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(Arg.Any<IEnumerable<JobHistoryError>>()).Throws(new Exception());
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(Arg.Any<Data.IntegrationPoint>()).Returns(true);
			_instance.IntegrationPoint.HasErrors = false;

			// Act
			Exception returnedException = Assert.Throws<Exception>(() => _instance.CommitErrors());

			// Assert
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Received().Update(Arg.Any<Data.IntegrationPoint>());
			Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Job    Error: Fake job error." + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Item    Identifier: MyIdentifier    Error: Fake item error."));
		}

		[Test]
		public void AddError_NoJobHistory_ThrowsException()
		{
			// Arrange
			_instance.JobHistory = null;

			// Act
			Exception returnedException = Assert.Throws<Exception>(() => _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

			// Assert
			_caseServiceContext.RsapiService.IntegrationPointLibrary.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.DidNotReceive().Create(Arg.Any<IEnumerable<JobHistoryError>>());
			Assert.That(returnedException.Message, Is.EqualTo("Type:Job  Id:  Error:Fake job error."));
		}

		[Test]
		public void OnRowError_DontAddErrorWhenStopped()
		{
			// ARRANGE
			const string identifier = "identifier";
			Reporter reporter = new Reporter();
			_stopJobManager.IsStoppingRequested().Returns(true);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			reporter.RaiseDocumentError(identifier, identifier);

			// ASSERT
			Assert.AreEqual(0, _instance.PendingErrorCount);
		}

		[Test]
		public void OnRowError_AddErrorWhenRunning()
		{
			// ARRANGE
			const string identifier = "identifier";
			Reporter reporter = new Reporter();
			_stopJobManager.IsStoppingRequested().Returns(false);

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
			_stopJobManager.IsStoppingRequested().Returns(true);

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
		public void OnJobError_AlwaysAddError(bool isStopped)
		{
			// ARRANGE
			Reporter reporter = new Reporter();
			Exception exception = new Exception();
			_stopJobManager.IsStoppingRequested().Returns(isStopped);

			// ACT
			_instance.SubscribeToBatchReporterEvents(reporter);
			reporter.RaiseOnJobError(exception);

			// ASSERT 
			Assert.AreEqual(1, _instance.PendingErrorCount);
		}

		[Test]
		public void CommitErrors_SurpressErrorOnUpdateHasErrorField()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(Arg.Any<Data.IntegrationPoint>()).Throws(new Exception());

			// ACT & ASSERT
			Assert.DoesNotThrow(() => _instance.CommitErrors());

		}

		private class Reporter : IBatchReporter
		{
			public event BatchCompleted OnBatchComplete;
			public event BatchSubmitted OnBatchSubmit;
			public event BatchCreated OnBatchCreate;
			public event StatusUpdate OnStatusUpdate;
			public event JobError OnJobError;
			public event RowError OnDocumentError;

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
		}
	}
}