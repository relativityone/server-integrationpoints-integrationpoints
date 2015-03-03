using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
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
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.");
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.");
			List<JobHistoryError> errors = new List<JobHistoryError>();
			context.RsapiService.JobHistoryErrorLibrary.Create(Arg.Do<IEnumerable<JobHistoryError>>(x => errors.AddRange(x)));


			//ACT
			jobHistoryErrorService.CommitErrors();


			//ASSERT
			Assert.AreEqual(2, errors.Count);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorJob.Name, errors[0].ErrorType.Name);
			Assert.AreEqual("Fake job error.", errors[0].Error);
			Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, errors[1].ErrorType.Name);
			Assert.AreEqual("Fake item error.", errors[1].Error);
		}

		[Test]
		public void CommitErrors_FailsCommit_ThrowsException()
		{
			//ARRANGE

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(null);
			jobHistoryErrorService.JobHistory = new JobHistory() { ArtifactId = 111 };
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.");
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.");
			List<JobHistoryError> errors = new List<JobHistoryError>();


			//ACT
			System.Exception returnedException = Assert.Throws<System.Exception>(() => jobHistoryErrorService.CommitErrors());


			//ASSERT
			Assert.That(returnedException.Message, Is.EqualTo("Could not commit Job History Errors. These are uncommited errors:" + Environment.NewLine
					+ "3/3/2015 11:27:00 PM Type: Job    Error: Fake job error." + Environment.NewLine
					+ "3/3/2015 11:27:00 PM Type: Item    Identifier: MyIdentifier    Error: Fake item error."));
		}

		[Test]
		public void AddError_NoJobHistory_ThrowsException()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<ICaseServiceContext>();

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(context);
			jobHistoryErrorService.JobHistory = null;


			//ACT
			System.Exception returnedException = Assert.Throws<System.Exception>(() => jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error."));


			//ASSERT
			Assert.That(returnedException.Message, Is.EqualTo("Type:Job  Id:  Error:Fake job error."));
		}
	}
}
