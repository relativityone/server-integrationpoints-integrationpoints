using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using User = kCura.IntegrationPoint.Tests.Core.User;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	[TestFixture]
	public class ItShouldRetrieveJobHistory : RelativityProviderTemplate
	{
		public ItShouldRetrieveJobHistory() : base($"SourceWorkspace_{Utils.FormatedDateTimeNow}", $"TargetWorkspace_{Utils.FormatedDateTimeNow}")
		{
		}

		private IList<TestData> _testData;
		private int _groupId;
		private UserModel _user;
		private RsapiClientLibrary<Data.IntegrationPoint> _integrationPointLibrary;
		private RsapiClientLibrary<JobHistory> _jobHistoryLibrary;

		private IList<TestData> _expectedResult;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointLibrary = new RsapiClientLibrary<Data.IntegrationPoint>(Helper, SourceWorkspaceArtifactId);
			_jobHistoryLibrary = new RsapiClientLibrary<JobHistory>(Helper, SourceWorkspaceArtifactId);

			_groupId = Group.CreateGroup($"group_{Utils.FormatedDateTimeNow}");
			_user = User.CreateUser("firstname", "lastname", $"a_{Utils.FormatedDateTimeNow}@kcura.com", new List<int> {_groupId});

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			_testData = TestData.Create(SourceWorkspaceArtifactId);
			_expectedResult = TestData.GetExpectedData(_testData);

			foreach (var testData in _testData)
			{
				if (!testData.PreventUserAccess)
				{
					Group.AddGroupToWorkspace(testData.WorkspaceId, _groupId);
				}

				CreateIntegrationPointAndAddJobHistory(testData);
			}
		}

		private void CreateIntegrationPointAndAddJobHistory(TestData testData)
		{
			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{new Guid()}", "Append Only");
			if (testData.IsLdapProvider)
			{
				integrationPointModel.SourceProvider = LdapProvider.ArtifactId;
			}

			var integrationPoint = CreateOrUpdateIntegrationPoint(integrationPointModel);

			var jobHistory = new JobHistory
			{
				Name = integrationPoint.Name,
				IntegrationPoint = new[] {integrationPoint.ArtifactID},
				BatchInstance = new Guid().ToString(),
				JobType = JobTypeChoices.JobHistoryRun,
				JobStatus = testData.JobHistoryStatus,
				ItemsTransferred = testData.DocsTransfered,
				TotalItems = testData.DocsTransfered,
				ItemsWithErrors = 0,
				DestinationWorkspace = testData.FullName,
				StartTimeUTC = DateTime.Now,
				EndTimeUTC = DateTime.Now
			};

			_jobHistoryLibrary.Create(jobHistory);

			if (testData.DeletedAfterRun)
			{
				_integrationPointLibrary.Delete(integrationPoint.ArtifactID);
			}
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();

			foreach (var testData in _testData)
			{
				ExceptionHelper.IgnoreExceptions(() => Workspace.DeleteWorkspace(testData.WorkspaceId));
			}
			ExceptionHelper.IgnoreExceptions(() => Group.DeleteGroup(_groupId));
			ExceptionHelper.IgnoreExceptions(() => User.DeleteUser(_user.ArtifactId));
		}

		[Test]
		public void ItShouldRetrieveHistory()
		{
			var client = Helper.CreateAdminProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 0,
				PageSize = 10
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(_expectedResult.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				Assert.That(_expectedResult.Any(x => x.DocsTransfered == jobHistoryModel.ItemsTransferred));
			}
		}

		[Test]
		public void ItShouldRetrieveHistoryUsingPaging()
		{
			var client = Helper.CreateAdminProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 1,
				PageSize = 1,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(1));

			var expectedTestData = _expectedResult[1];
			Assert.That(jobHistory.Data[0].DestinationWorkspace, Is.EqualTo(expectedTestData.FullName));
			Assert.That(jobHistory.Data[0].ItemsTransferred, Is.EqualTo(expectedTestData.DocsTransfered));
		}

		[Test]
		public void ItShouldHandleRetrievingHistoryFromOutsideTheRange()
		{
			var client = Helper.CreateAdminProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 10,
				PageSize = 10,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(0));
		}

		[Test]
		public void ItShouldRetrieveHistoryRespectingPermission()
		{
			var client = Helper.CreateUserProxy<IJobHistoryManager>(_user.EmailAddress);
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 0,
				PageSize = 10,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			var testDataAfterPermission = _expectedResult.Where(x => !x.PreventUserAccess).ToList();

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(testDataAfterPermission.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(testDataAfterPermission.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(testDataAfterPermission.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				Assert.That(testDataAfterPermission.Any(x => x.DocsTransfered == jobHistoryModel.ItemsTransferred));
			}
		}

		[Test]
		[TestCase(nameof(JobHistoryModel.DestinationWorkspace), true)]
		[TestCase(nameof(JobHistoryModel.DestinationWorkspace), false)]
		[TestCase(nameof(JobHistoryModel.EndTimeUTC), true)]
		[TestCase(nameof(JobHistoryModel.EndTimeUTC), false)]
		[TestCase(nameof(JobHistoryModel.ItemsTransferred), true)]
		[TestCase(nameof(JobHistoryModel.ItemsTransferred), false)]
		public void ItShouldSortResult(string propertyName, bool sortDescending)
		{
			var client = Helper.CreateAdminProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 0,
				PageSize = 10,
				SortColumnName = propertyName,
				SortDescending = sortDescending
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			if (sortDescending)
			{
				Assert.That(jobHistory.Data, Is.Ordered.Descending.By(propertyName));
			}
			else
			{
				Assert.That(jobHistory.Data, Is.Ordered.By(propertyName));
			}
		}

		private class TestData
		{
			public int WorkspaceId { get; private set; }
			private string WorkspaceName { get; set; }
			public int DocsTransfered { get; private set; }
			public string FullName => $"{WorkspaceName} - {WorkspaceId}";
			public bool PreventUserAccess { get; set; }
			public bool DeletedAfterRun { get; set; }
			public bool IsLdapProvider { get; set; }
			public Choice JobHistoryStatus { get; set; }

			public static IList<TestData> Create(int sourceWorkspaceId)
			{
				var workspaceName = $"target_1_{Utils.FormatedDateTimeNow}";
				var workspaceId = Workspace.CreateWorkspace(workspaceName, WorkspaceTemplates.NEW_CASE_TEMPLATE);

				var workspaceNoAccessName = $"target_2_{Utils.FormatedDateTimeNow}";
				var workspaceNoAccessId = Workspace.CreateWorkspace(workspaceNoAccessName, WorkspaceTemplates.NEW_CASE_TEMPLATE);

				var testCase1 = new TestData
				{
					WorkspaceName = workspaceNoAccessName,
					WorkspaceId = workspaceNoAccessId,
					DocsTransfered = 17,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompleted,
					PreventUserAccess = true
				};
				var testCase2 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 11,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompletedWithErrors
				};
				var testCase3 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 31,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
					DeletedAfterRun = true
				};
				var testCase4 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 23,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompleted
				};
				var testCase5 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 59,
					JobHistoryStatus = JobStatusChoices.JobHistoryStopped
				};
				var testCase6 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 47,
					JobHistoryStatus = JobStatusChoices.JobHistoryStopping
				};
				var testCase7 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 41,
					JobHistoryStatus = JobStatusChoices.JobHistoryErrorJobFailed
				};
				var testCase8 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 61,
					JobHistoryStatus = JobStatusChoices.JobHistoryPending
				};
				var testCase9 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransfered = 53,
					JobHistoryStatus = JobStatusChoices.JobHistoryProcessing
				};
				var testCase10 = new TestData
				{
					WorkspaceName = "workspace",
					WorkspaceId = sourceWorkspaceId,
					DocsTransfered = 29,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompleted,
					IsLdapProvider = true
				};

				var testData = new List<TestData>
				{
					testCase1,
					testCase2,
					testCase3,
					testCase4,
					testCase5,
					testCase6,
					testCase7,
					testCase8,
					testCase9,
					testCase10
				};

				return testData;
			}

			public static IList<TestData> GetExpectedData(IList<TestData> testData)
			{
				return testData.Where(x => !x.DeletedAfterRun)
					.Where(x => !x.IsLdapProvider)
					.Where(x => (x.JobHistoryStatus == JobStatusChoices.JobHistoryCompleted) || (x.JobHistoryStatus == JobStatusChoices.JobHistoryCompletedWithErrors))
					.ToList();
			}
		}
	}
}