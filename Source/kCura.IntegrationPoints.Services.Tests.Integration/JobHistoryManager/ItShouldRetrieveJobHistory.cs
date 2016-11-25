using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	public class ItShouldRetrieveJobHistory : RelativityProviderTemplate
	{
		public ItShouldRetrieveJobHistory() : base($"SourceWorkspace_{Utils.Identifier}", $"TargetWorkspace_{Utils.Identifier}")
		{
		}

		private IList<TestData> _testData;
		private int _groupId;
		private UserModel _user;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_groupId = Group.CreateGroup($"group_{Utils.Identifier}");
			_user = User.CreateUser("firstname", "lastname", $"a_{Utils.Identifier}@kcura.com", new List<int> {_groupId});

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			_testData = TestData.Create();

			foreach (var testData in _testData)
			{
				ExecuteTestData(testData);
			}
		}

		private void ExecuteTestData(TestData testData)
		{
			var importTableForTagging = Import.GetImportTable(testData.DocPrefix, testData.DocsTransfered);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTableForTagging);

			var ipModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{Utils.Identifier}", "Append Only");
			ipModel.SourceConfiguration = CreateSourceConfigWithTargetWorkspace(testData.WorkspaceId);
			ipModel.Destination = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, testData.WorkspaceId);

			var ip = CreateOrUpdateIntegrationPoint(ipModel);

			SavedSearch.ModifySavedSearchByAddingPrefix(Container, SourceWorkspaceArtifactId, SavedSearchArtifactId, testData.DocPrefix, true);

			var service = Container.Resolve<IIntegrationPointService>();
			service.RunIntegrationPoint(SourceWorkspaceArtifactId, ip.ArtifactID, 9);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, ip.ArtifactID);

			if (!testData.PreventUserAcces)
			{
				Group.AddGroupToWorkspace(testData.WorkspaceId, _groupId);
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

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_testData.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_testData.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(_testData.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				var testData = _testData.First(x => x.FullName == jobHistoryModel.DestinationWorkspace);
				Assert.That(jobHistoryModel.ItemsTransferred, Is.EqualTo(testData.DocsTransfered));
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

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_testData.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_testData.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(1));

			var expectedTestData = _testData[1];
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

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(_testData.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(_testData.Count));
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

			var testDataAfterPermission = _testData.Where(x => !x.PreventUserAcces).ToList();

			Assert.That(jobHistory.TotalDocumentsPushed, Is.EqualTo(testDataAfterPermission.Sum(x => x.DocsTransfered)));
			Assert.That(jobHistory.TotalAvailable, Is.EqualTo(testDataAfterPermission.Count));
			Assert.That(jobHistory.Data.Length, Is.EqualTo(testDataAfterPermission.Count));

			foreach (var jobHistoryModel in jobHistory.Data)
			{
				var testData = testDataAfterPermission.First(x => x.FullName == jobHistoryModel.DestinationWorkspace);
				Assert.That(jobHistoryModel.ItemsTransferred, Is.EqualTo(testData.DocsTransfered));
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
			public string DocPrefix => WorkspaceName;
			public int DocsTransfered { get; private set; }
			public string FullName => $"{WorkspaceName} - {WorkspaceId}";
			public bool PreventUserAcces { get; set; }

			public static IList<TestData> Create()
			{
				var workspace1 = new TestData
				{
					WorkspaceName = $"target_1_{Utils.Identifier}",
					DocsTransfered = 17,
					PreventUserAcces = true
				};
				var workspace2 = new TestData
				{
					WorkspaceName = $"target_2_{Utils.Identifier}",
					DocsTransfered = 11,
					PreventUserAcces = false
				};
				var workspace3 = new TestData
				{
					WorkspaceName = $"target_3_{Utils.Identifier}",
					DocsTransfered = 23,
					PreventUserAcces = false
				};

				var testData = new List<TestData> {workspace1, workspace2, workspace3};

				foreach (var data in testData)
				{
					data.WorkspaceId = Workspace.CreateWorkspace(data.WorkspaceName, WorkspaceTemplates.NEW_CASE_TEMPLATE);
				}

				return testData;
			}
		}
	}
}