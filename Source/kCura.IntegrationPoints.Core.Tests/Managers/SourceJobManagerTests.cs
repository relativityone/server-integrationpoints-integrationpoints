using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class SourceJobManagerTests : TestBase
	{
		private const int _SOURCE_WORKSPACE_ID = 629272;
		private const int _DESTINATION_WORKSPACE_ID = 326325;

		private ISourceJobRepository _sourceJobRepository;
		private IRdoRepository _sourceRdoRepository;

		private SourceJobManager _instance;

		public override void SetUp()
		{
			_sourceJobRepository = Substitute.For<ISourceJobRepository>();
			_sourceRdoRepository = Substitute.For<IRdoRepository>();

			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();

			repositoryFactory.GetRdoRepository(_SOURCE_WORKSPACE_ID).Returns(_sourceRdoRepository);
			repositoryFactory.GetSourceJobRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceJobRepository);

			_instance = new SourceJobManager(repositoryFactory);
		}

		[Test]
		public void ItShouldCreateInstance()
		{
			int jobHistoryArtifactId = 134703;
			int sourceWorkspaceRdoInstanceArtifactId = 966631;
			int sourceJobDescriptorArtifactTypeId = 577930;

			RDO jobHistoryRdo = new RDO
			{
				TextIdentifier = "job_history_name_956"
			};
			var sourceJobId = 713321;

			_sourceRdoRepository.ReadSingle(jobHistoryArtifactId).Returns(jobHistoryRdo);
			_sourceJobRepository.Create(Arg.Any<SourceJobDTO>()).Returns(sourceJobId);

			//ACT
			var sourceJobDto = _instance.CreateSourceJobDto(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, jobHistoryArtifactId, sourceWorkspaceRdoInstanceArtifactId,
				sourceJobDescriptorArtifactTypeId);

			// ASSERT
			ValidateSourceJob(sourceJobDto, sourceJobId, jobHistoryRdo.TextIdentifier, jobHistoryArtifactId, sourceWorkspaceRdoInstanceArtifactId);
		}

		private void ValidateSourceJob(SourceJobDTO sourceJob, int sourceJobArtifactId, string jobHistoryName, int jobHistoryId, int sourceWorkspaceRdoInstanceArtifactId)
		{
			Assert.IsNotNull(sourceJob);
			string expectedName = Utils.GetFormatForWorkspaceOrJobDisplay(jobHistoryName, jobHistoryId);
			Assert.AreEqual(expectedName, sourceJob.Name);
			Assert.AreEqual(sourceWorkspaceRdoInstanceArtifactId, sourceJob.SourceWorkspaceArtifactId);
			Assert.AreEqual(jobHistoryId, sourceJob.JobHistoryArtifactId);
			Assert.AreEqual(jobHistoryName, sourceJob.JobHistoryName);
			Assert.AreEqual(sourceJobArtifactId, sourceJob.ArtifactId);
		}
	}
}