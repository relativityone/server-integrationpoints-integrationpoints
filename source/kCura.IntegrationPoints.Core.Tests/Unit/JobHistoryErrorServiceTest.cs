﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class JobHistoryErrorServiceTest
	{
		[Test]
		public void CommitErrors_HasJobHistory_CommitsJobHistoryErrors()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<ICaseServiceContext>();

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(context);
			jobHistoryErrorService.JobHistory = new JobHistory() { ArtifactId = 111 };
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null);
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");
			List<JobHistoryError> errors = new List<JobHistoryError>();
			context.RsapiService.JobHistoryErrorLibrary.Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			jobHistoryErrorService.IntegrationPoint = new Data.IntegrationPoint();
			jobHistoryErrorService.IntegrationPoint.HasErrors = false;

			//ACT
			jobHistoryErrorService.CommitErrors();

			//ASSERT
			context.RsapiService.JobHistoryErrorLibrary.Received().Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));
			Assert.AreEqual(2, errors.Count);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorJob.Name, errors[0].ErrorType.Name);
			Assert.AreEqual("Fake job error.", errors[0].Error);
			Assert.AreEqual(null, errors[0].StackTrace);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, errors[1].ErrorType.Name);
			Assert.AreEqual("Fake item error.", errors[1].Error);
			Assert.AreEqual("stack trace", errors[1].StackTrace);
			Assert.IsTrue(jobHistoryErrorService.IntegrationPoint.HasErrors.Value);
		}

		[Test]
		public void CommitErrors_HasJobHistory_NoErrorsToCommit()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<ICaseServiceContext>();

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(context);
			jobHistoryErrorService.JobHistory = new JobHistory() { ArtifactId = 111 };
			jobHistoryErrorService.IntegrationPoint = new Data.IntegrationPoint();
			jobHistoryErrorService.IntegrationPoint.HasErrors = true;

			//ACT
			jobHistoryErrorService.CommitErrors();

			//ASSERT
			context.RsapiService.JobHistoryErrorLibrary.DidNotReceive().Create(Arg.Any<IEnumerable<JobHistoryError>>());
			Assert.IsFalse(jobHistoryErrorService.IntegrationPoint.HasErrors.Value);
		}

		[Test]
		public void CommitErrors_FailsCommit_ThrowsException()
		{
			//ARRANGE
			var context = Substitute.For<ICaseServiceContext>();

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(context);
			jobHistoryErrorService.JobHistory = new JobHistory() { ArtifactId = 111 };
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null);
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", null);
			context.RsapiService.JobHistoryErrorLibrary.Create(Arg.Any<IEnumerable<JobHistoryError>>()).Throws(new Exception());
			context.RsapiService.IntegrationPointLibrary.Update(Arg.Any<Data.IntegrationPoint>()).Returns(true);
			jobHistoryErrorService.IntegrationPoint = new Data.IntegrationPoint();
			jobHistoryErrorService.IntegrationPoint.HasErrors = false;

			//ACT
			System.Exception returnedException = Assert.Throws<System.Exception>(() => jobHistoryErrorService.CommitErrors());

			//ASSERT
			context.RsapiService.IntegrationPointLibrary.Received().Update(Arg.Any<Data.IntegrationPoint>());
			Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Job    Error: Fake job error." + Environment.NewLine));
			Assert.IsTrue(returnedException.Message.Contains("Type: Item    Identifier: MyIdentifier    Error: Fake item error."));
		}

		[Test]
		public void AddError_NoJobHistory_ThrowsException()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<ICaseServiceContext>();

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(context);
			jobHistoryErrorService.JobHistory = null;

			//ACT
			System.Exception returnedException = Assert.Throws<System.Exception>(() => jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

			//ASSERT
			context.RsapiService.IntegrationPointLibrary.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
			context.RsapiService.JobHistoryErrorLibrary.DidNotReceive().Create(Arg.Any<IEnumerable<JobHistoryError>>());
			Assert.That(returnedException.Message, Is.EqualTo("Type:Job  Id:  Error:Fake job error."));
		}
	}
}