using System;
using System.Collections.Generic;
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

		[SetUp]
		public void SetUp()
		{
			_integrationPoint = new Data.IntegrationPoint();
			_jobHistory = new Data.JobHistory { ArtifactId = 111 };

			_caseServiceContext = Substitute.For<ICaseServiceContext>();

			_instance = new JobHistoryErrorService(_caseServiceContext)
			{
				IntegrationPoint = _integrationPoint,
				JobHistory = _jobHistory
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
	}
}