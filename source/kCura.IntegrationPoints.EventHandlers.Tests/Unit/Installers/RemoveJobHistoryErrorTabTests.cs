using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Installers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.Installers
{
	[TestFixture]
	public class RemoveJobHistoryErrorTabTests
	{
		private IRepositoryFactory _repositoryFactory;
		private IDBContext _workspaceDbContext;
		private ITabRepository _tabRepository;
		private RemoveJobHistoryErrorTab _instance;
		private int _workspaceId = 12345;
		private string _jobHistoryTabGuid = "FD585DBF-98EA-427B-8CE5-3E09A053DC14";
		private readonly int? _tabArtifactId = 98765;
		private string _successMessage = "Succesfully removed Job History Error tab";
		private string _failureMessage = "Unable to retrieve tab";

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_workspaceDbContext = Substitute.For<IDBContext>();
			_tabRepository = Substitute.For<ITabRepository>();

			_instance = new RemoveJobHistoryErrorTab(_repositoryFactory, _workspaceDbContext);
		}

		[Test]
		public void RemoveJobHistoryErrorTab_GoldFlow_TabExists()
		{
			//Arrange
			string sql = String.Format(@"DELETE FROM [ApplicationTab] WHERE [TabArtifactID] = {0}", _tabArtifactId);
			_repositoryFactory.GetTabRepository(_workspaceId).Returns(_tabRepository);
			_tabRepository.RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid).Returns(_tabArtifactId);
			_workspaceDbContext.ExecuteNonQuerySQLStatement(Arg.Is(sql)).Returns(0);
			
			
			//Act
			kCura.EventHandler.Response retVal = _instance.ExecuteInstanced(_workspaceId);
			
			//Assert
			_repositoryFactory.Received().GetTabRepository(_workspaceId);
			_tabRepository.Received().RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid);
			_workspaceDbContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
			_tabRepository.Received().Delete(_tabArtifactId.Value);

			Assert.IsTrue(retVal.Success);
			Assert.IsTrue(retVal.Message == _successMessage);
		}

		[Test]
		public void RemoveJobHistoryErrorTab_TabDoesntExist()
		{
			//Arrange
			int? tabId = null;
			_repositoryFactory.GetTabRepository(_workspaceId).Returns(_tabRepository);
			_tabRepository.RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid).Returns(tabId);

			//Act
			kCura.EventHandler.Response retVal = _instance.ExecuteInstanced(_workspaceId);

			//Assert
			_repositoryFactory.Received().GetTabRepository(_workspaceId);
			_tabRepository.Received().RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid);

			_workspaceDbContext.DidNotReceive().ExecuteNonQuerySQLStatement(Arg.Any<string>());
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			Assert.IsTrue(retVal.Success);
			Assert.IsTrue(retVal.Message == _successMessage);
		}

		[Test]
		public void RemoveJobHistoryErrorTab_RetrieveTabFails()
		{
			//Arrange
			Exception ex = new Exception(_failureMessage);
			_repositoryFactory.GetTabRepository(_workspaceId).Returns(_tabRepository);
			_tabRepository.RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid).Throws(ex);

			//Act
			kCura.EventHandler.Response retVal = _instance.ExecuteInstanced(_workspaceId);

			//Assert
			_repositoryFactory.Received().GetTabRepository(_workspaceId);
			_tabRepository.Received().RetrieveTabArtifactIdByGuid(_jobHistoryTabGuid);

			_workspaceDbContext.DidNotReceive().ExecuteNonQuerySQLStatement(Arg.Any<string>());
			_tabRepository.DidNotReceive().Delete(Arg.Any<int>());

			Assert.IsFalse(retVal.Success);
			Assert.IsTrue(retVal.Message == _failureMessage);
		}
	}
}
