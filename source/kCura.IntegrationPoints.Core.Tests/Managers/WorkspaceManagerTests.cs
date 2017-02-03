using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;


namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class WorkspaceManagerTests
	{
		private const int CurrentUserWorkspaceArtifactId = 1234;
		private IRepositoryFactory _repositoryFactory;
		private IWorkspaceRepository _workspaceRepository;
		private IRdoRepository _rdoRepository;
		private int _workspaceArtifactId;

		[SetUp]
		public void Setup()
		{
			_workspaceArtifactId = -1;

			QueryResultSet<RDO> rdoQueryResultSet = new QueryResultSet<RDO>
			{
				Success = true,
				Results = new List<Result<RDO>>() { new Result<RDO>() {Artifact = new RDO(CurrentUserWorkspaceArtifactId)}}
			};

			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_workspaceRepository.RetrieveAllActive()
				.Returns(new List<WorkspaceDTO>()
				{
					new WorkspaceDTO() {ArtifactId = CurrentUserWorkspaceArtifactId, Name = "Test Workspace"},
					new WorkspaceDTO() {ArtifactId = 5678, Name = "Admin User Workspace"}
				});

			_workspaceRepository.RetrieveAll()
				.Returns(new List<WorkspaceDTO>()
				{
					new WorkspaceDTO() { ArtifactId = CurrentUserWorkspaceArtifactId, Name = "Test Workspace"},
					new WorkspaceDTO {ArtifactId = 9012, Name = "I am being upgraded"}
				});

			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);

			_rdoRepository = Substitute.For<IRdoRepository>();
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(rdoQueryResultSet);
			_repositoryFactory.GetRdoRepository(_workspaceArtifactId).Returns(_rdoRepository);
		}

		[Test]
		public void It_should_return_workspaces_only_accessible_for_current_user()
		{
			//ARRANGE
			WorkspaceManager workspaceManager = new WorkspaceManager(_repositoryFactory);

			//ACT
			IEnumerable<WorkspaceDTO> userWorkspaces = workspaceManager.GetUserActiveWorkspaces().ToList();

			//ASSERT
			Assert.AreEqual(1, userWorkspaces.Count());
			Assert.AreEqual(CurrentUserWorkspaceArtifactId, userWorkspaces.First().ArtifactId);
		}
	}
}