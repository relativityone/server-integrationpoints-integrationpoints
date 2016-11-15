using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class JobHistoryErrorRepositoryTests : TestBase
	{
		private JobHistoryErrorRepository _instance;
		private IHelper _helper;
		private IObjectQueryManagerAdaptor _objectQueryAdaptorManager;
		private IGenericLibrary<JobHistoryError> _jobHistoryErrorLibrary;
		private IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _dtoTransformer;
		private int _workspaceArtifactId;

		[SetUp]
		public override void SetUp()
		{
			_helper = NSubstitute.Substitute.For<IHelper>();
			_objectQueryAdaptorManager = NSubstitute.Substitute.For<IObjectQueryManagerAdaptor>();
			_jobHistoryErrorLibrary = NSubstitute.Substitute.For<IGenericLibrary<JobHistoryError>>();
			_dtoTransformer = NSubstitute.Substitute.For<IDtoTransformer<JobHistoryErrorDTO, JobHistoryError>>();
			_workspaceArtifactId = 456;
			_instance = new JobHistoryErrorRepository(_helper, 
				_objectQueryAdaptorManager, 
				_jobHistoryErrorLibrary,
				_dtoTransformer,
				_workspaceArtifactId);
		}

		#region Read
		private static IEnumerable<int>[] ReadSource = {
			new List<int>(),
			new List<int>() { 1 },
			new List<int>() { 1 , 2, 3 , 4 } ,
			null
		};

		[Test, TestCaseSource(nameof(ReadSource))]
		public void Read(IEnumerable<int> artifactIds)
		{
			// act
			_instance.Read(artifactIds);

			// assert
			_jobHistoryErrorLibrary.Received(1).Read(artifactIds);
		}
		#endregion

		#region DeleteItemLevelErrorsSavedSearch
		private static List<Task>[] DeleteItemLevelErrorsSavedSearch = {
			new List<Task>() { CreateCompletedTask() },
			new List<Task>() { CreateErrorTask(), CreateCompletedTask()},
			new List<Task>() { CreateErrorTask(), CreateErrorTask(), CreateErrorTask() },
			new List<Task>() { CreateErrorTask(), CreateErrorTask(), CreateCompletedTask() },
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