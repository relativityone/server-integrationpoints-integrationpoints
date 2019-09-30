using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Testing.Identification;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using User = kCura.IntegrationPoint.Tests.Core.User;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldRetrieveJobHistory : RelativityProviderTemplate
	{
		public ItShouldRetrieveJobHistory() : base($"SourceWorkspace_{Utils.FormattedDateTimeNow}", $"TargetWorkspace_{Utils.FormattedDateTimeNow}")
		{
		}

		private IList<TestData> _testData;
		private int _groupId;
		private UserModel _user;
		private IRelativityObjectManager _objectManager;

		private IList<TestData> _expectedResult;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_objectManager = CreateObjectManager();

			_groupId = Group.CreateGroup($"group_{Utils.FormattedDateTimeNow}");
			_user = User.CreateUser("firstname", "lastname", $"a_{Utils.FormattedDateTimeNow}@relativity.com", new List<int> {_groupId});

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactID, _groupId);

			_testData = TestData.Create(SourceWorkspaceArtifactID);
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

		private IRelativityObjectManager CreateObjectManager()
		{
			var factory = new RelativityObjectManagerFactory(Helper);
			return factory.CreateRelativityObjectManager(SourceWorkspaceArtifactID);
		}

		private void CreateIntegrationPointAndAddJobHistory(TestData testData)
		{
			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{new Guid()}", "Append Only");
			if (testData.IsLdapProvider)
			{
				integrationPointModel.SourceProvider = LdapProvider.ArtifactId;
				integrationPointModel.Type = Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid)
					.ArtifactId;
			}

			var integrationPoint = CreateOrUpdateIntegrationPoint(integrationPointModel);

			var jobHistory = new JobHistory
			{
				Name = integrationPoint.Name,
				IntegrationPoint = new[] {integrationPoint.ArtifactID},
				BatchInstance = new Guid().ToString(),
				JobType = JobTypeChoices.JobHistoryRun,
				JobStatus = testData.JobHistoryStatus,
				ItemsTransferred = testData.DocsTransferred,
				TotalItems = testData.DocsTransferred,
				ItemsWithErrors = 0,
				DestinationWorkspace = testData.FullName,
				DestinationInstance = FederatedInstanceManager.LocalInstance.Name,
				StartTimeUTC = DateTime.Now,
				EndTimeUTC = DateTime.Now
			};

			_objectManager.Create(jobHistory);

			if (testData.DeletedAfterRun)
			{
				IntegrationPointRepository.Delete(integrationPoint.ArtifactID);
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
			ExceptionHelper.IgnoreExceptions(() => User.DeleteUser(_user.ArtifactID));
		}

		[IdentifiedTest("5d55e45e-e4bd-407e-9daf-bc5ce33879ab")]
		public void ItShouldRetrieveHistory()
		{
			var client = Helper.CreateProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 0,
				PageSize = 10
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransferred)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(_expectedResult.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				Assert.That(_expectedResult.Any(x => x.DocsTransferred == jobHistoryModel.ItemsTransferred));
			}
		}

		[IdentifiedTest("700c2132-4dd5-4e98-a630-06618849935f")]
		public void ItShouldRetrieveHistoryUsingPaging()
		{
			var client = Helper.CreateProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 1,
				PageSize = 1,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransferred)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(1));

			var expectedTestData = _expectedResult[1];
			Assert.That(jobHistory.Data[0].DestinationWorkspace, Is.EqualTo(expectedTestData.FullName));
			Assert.That(jobHistory.Data[0].ItemsTransferred, Is.EqualTo(expectedTestData.DocsTransferred));
		}

		[IdentifiedTest("c736309c-1f09-4a69-a436-ebd109610b0d")]
		public void ItShouldHandleRetrievingHistoryFromOutsideTheRange()
		{
			var client = Helper.CreateProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 10,
				PageSize = 10,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_expectedResult.Sum(x => x.DocsTransferred)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_expectedResult.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(0));
		}

		[IdentifiedTest("6db36b99-49d6-49c6-a16c-7113fe0e26f1")]
		public void ItShouldRetrieveHistoryRespectingPermission()
		{
			var client = Helper.CreateProxy<IJobHistoryManager>(_user.EmailAddress);
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 0,
				PageSize = 10,
				SortColumnName = nameof(JobHistoryModel.EndTimeUTC)
			};
			var jobHistory = client.GetJobHistoryAsync(request).Result;

			var testDataAfterPermission = _expectedResult.Where(x => !x.PreventUserAccess).ToList();

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(testDataAfterPermission.Sum(x => x.DocsTransferred)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(testDataAfterPermission.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(testDataAfterPermission.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				Assert.That(testDataAfterPermission.Any(x => x.DocsTransferred == jobHistoryModel.ItemsTransferred));
			}
		}

		[IdentifiedTestCase("17983c3a-db58-4de1-8f7d-420eddbb7759", nameof(JobHistoryModel.DestinationWorkspace), true)]
		[IdentifiedTestCase("c7bbe0ae-af5e-489f-97d0-27a5cf3bb7e0", nameof(JobHistoryModel.DestinationWorkspace), false)]
		[IdentifiedTestCase("b4a0f92f-b434-4aa3-bd2d-4d4164f4cce2", nameof(JobHistoryModel.EndTimeUTC), true)]
		[IdentifiedTestCase("6aee68f4-5690-4c1a-9981-d034a5aa6056", nameof(JobHistoryModel.EndTimeUTC), false)]
		[IdentifiedTestCase("5469bc44-b4df-4808-900b-35384a09d89f", nameof(JobHistoryModel.ItemsTransferred), true)]
		[IdentifiedTestCase("994a6e18-3aa0-4c1c-b5bf-388b9f467d42", nameof(JobHistoryModel.ItemsTransferred), false)]
		public void ItShouldSortResult(string propertyName, bool sortDescending)
		{
			var client = Helper.CreateProxy<IJobHistoryManager>();
			var request = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
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
			public int DocsTransferred { get; private set; }
			public string FullName => $"{FederatedInstanceManager.LocalInstance.Name} - {WorkspaceName} - {WorkspaceId}";
			public bool PreventUserAccess { get; set; }
			public bool DeletedAfterRun { get; set; }
			public bool IsLdapProvider { get; set; }
			public Choice JobHistoryStatus { get; set; }

			public static IList<TestData> Create(int sourceWorkspaceId)
			{
				var workspaceName = $"target_1_{Utils.FormattedDateTimeNow}";
				var workspaceId = Workspace.CreateWorkspace(workspaceName);

				var workspaceNoAccessName = $"target_2_{Utils.FormattedDateTimeNow}";
				var workspaceNoAccessId = Workspace.CreateWorkspace(workspaceNoAccessName);

				var testCase1 = new TestData
				{
					WorkspaceName = workspaceNoAccessName,
					WorkspaceId = workspaceNoAccessId,
					DocsTransferred = 17,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompleted,
					PreventUserAccess = true
				};
				var testCase2 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 11,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompletedWithErrors
				};
				var testCase3 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 31,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
					DeletedAfterRun = true
				};
				var testCase4 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 23,
					JobHistoryStatus = JobStatusChoices.JobHistoryCompleted
				};
				var testCase5 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 59,
					JobHistoryStatus = JobStatusChoices.JobHistoryStopped
				};
				var testCase6 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 47,
					JobHistoryStatus = JobStatusChoices.JobHistoryStopping
				};
				var testCase7 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 41,
					JobHistoryStatus = JobStatusChoices.JobHistoryErrorJobFailed
				};
				var testCase8 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 61,
					JobHistoryStatus = JobStatusChoices.JobHistoryPending
				};
				var testCase9 = new TestData
				{
					WorkspaceName = workspaceName,
					WorkspaceId = workspaceId,
					DocsTransferred = 53,
					JobHistoryStatus = JobStatusChoices.JobHistoryProcessing
				};
				var testCase10 = new TestData
				{
					WorkspaceName = "workspace",
					WorkspaceId = sourceWorkspaceId,
					DocsTransferred = 29,
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