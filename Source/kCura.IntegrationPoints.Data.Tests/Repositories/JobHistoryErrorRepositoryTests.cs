using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class JobHistoryErrorRepositoryTests : TestBase
	{
		private JobHistoryErrorRepository _instance;
		private IHelper _helper;
		private IRelativityObjectManager _objectManager;
		private int _workspaceArtifactId;

		[SetUp]
		public override void SetUp()
		{
			_helper = NSubstitute.Substitute.For<IHelper>();
			_objectManager = NSubstitute.Substitute.For<IRelativityObjectManager>();
			_workspaceArtifactId = 456;
			var objectManagerFactory = Substitute.For<IRelativityObjectManagerFactory>();
			objectManagerFactory.CreateRelativityObjectManager(Arg.Any<int>()).Returns(_objectManager);
			_instance = new JobHistoryErrorRepository(_helper, objectManagerFactory, _workspaceArtifactId);
		}

		#region Read
		private static IEnumerable<int>[] ReadSource = {
			new List<int>(),
			new List<int> { 1 },
			new List<int> { 1 , 2, 3 , 4 }
		};

		[Test, TestCaseSource(nameof(ReadSource))]
		public void Read(IEnumerable<int> artifactIds)
		{
			// act
			_instance.Read(artifactIds);

			// assert
			_objectManager.Received(1).Query<JobHistoryError>(Arg.Is<QueryRequest>( x => x.Condition == $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", artifactIds)}]"));
		}
		#endregion

		#region DeleteItemLevelErrorsSavedSearch
		private static List<Task>[] DeleteItemLevelErrorsSavedSearch = {
			new List<Task> { CreateCompletedTask() },
			new List<Task> { CreateErrorTask(), CreateCompletedTask()},
			new List<Task> { CreateErrorTask(), CreateErrorTask(), CreateErrorTask() },
			new List<Task> { CreateErrorTask(), CreateErrorTask(), CreateCompletedTask() },
		};

		private static Task CreateCompletedTask()
		{
			Task task = new Task(() => { });
			task.Start();
			task.Wait();
			return task;
		}

		private static Task CreateErrorTask()
		{
			Task task = null;
			try
			{
				task = new Task(() => { throw new Exception(); });
				task.Start();
				task.Wait();
			}
			catch (Exception)
			{ }
			return task;
		}

		[Test, TestCaseSource(nameof(DeleteItemLevelErrorsSavedSearch))]
		public void DeleteItemLevelErrorsSavedSearch_GoldFlow(List<Task> mockTasks)
		{
			// arrange
			IKeywordSearchManager keywordSearch = Substitute.For<IKeywordSearchManager>();

			int searchId = 123456;
			_helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(Arg.Any<ExecutionIdentity>()).Returns(keywordSearch);
			if (mockTasks.Count < 2)
			{
				keywordSearch.DeleteSingleAsync(_workspaceArtifactId, searchId).Returns(mockTasks[0]);
			}
			else
			{
				keywordSearch.DeleteSingleAsync(_workspaceArtifactId, searchId).Returns(mockTasks[0], mockTasks.GetRange(1, mockTasks.Count - 1).ToArray());
			}

			// act
			_instance.DeleteItemLevelErrorsSavedSearch(searchId);

			// assert
			keywordSearch.Received(mockTasks.Count).DeleteSingleAsync(_workspaceArtifactId, searchId);
		}

		#endregion
	}
}