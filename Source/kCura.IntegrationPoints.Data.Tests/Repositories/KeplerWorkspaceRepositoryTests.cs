﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class KeplerWorkspacesRepositoryTests
	{
		private IHelper _helper;
		private IServicesMgr _servicesMgr;
		private IWorkspaceManager _workspaceManagerProxy;
		private IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_servicesMgr = Substitute.For<IServicesMgr>();
			_workspaceManagerProxy = Substitute.For<IWorkspaceManager>();
			_objectQueryManagerAdaptor = Substitute.For<IObjectQueryManagerAdaptor>();
		}

		[Test]
		public void ItShouldRetrieveAllActive()
		{
			//Arrange 
			const int workspaceId = 0;
			const string workspaceName = "New Workspace";
			var workspaces = new List<WorkspaceRef>();
			workspaces.Add(new WorkspaceRef() { ArtifactID = workspaceId, Name = workspaceName });
			_workspaceManagerProxy.RetrieveAllActive().Returns(workspaces);
			_servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.CurrentUser).Returns(_workspaceManagerProxy);
			var repository = new KeplerWorkspaceRepository(_helper, _servicesMgr, _objectQueryManagerAdaptor);

			//Act
			List<WorkspaceDTO> resultList = repository.RetrieveAllActive().ToList();

			//Assert
			Assert.AreEqual(resultList.Count, 1);
			Assert.AreEqual(resultList.First().Name, workspaceName);
			Assert.AreEqual(resultList.First().ArtifactId, workspaceId);
		}
	}
}
