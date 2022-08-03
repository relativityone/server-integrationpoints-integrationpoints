using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class SourceJobManagerTests : TestBase
    {
        private const int _SOURCE_WORKSPACE_ID = 629272;
        private const int _DESTINATION_WORKSPACE_ID = 326325;

        private ISourceJobRepository _sourceJobRepository;
        private IJobHistoryRepository _jobHistoryRepository;

        private SourceJobManager _instance;

        public override void SetUp()
        {
            _sourceJobRepository = Substitute.For<ISourceJobRepository>();
            _jobHistoryRepository = Substitute.For<IJobHistoryRepository>();

            IHelper helper = Substitute.For<IHelper>();
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();

            repositoryFactory.GetJobHistoryRepository(_SOURCE_WORKSPACE_ID).Returns(_jobHistoryRepository);
            repositoryFactory.GetSourceJobRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceJobRepository);

            _instance = new SourceJobManager(repositoryFactory, helper);
        }

        [Test]
        public void ItShouldCreateInstance()
        {
            int jobHistoryArtifactId = 134703;
            int sourceWorkspaceRdoInstanceArtifactId = 966631;

            string jobHistoryName = "job_history_name_956";
            var sourceJobId = 713321;

            _jobHistoryRepository.GetJobHistoryName(jobHistoryArtifactId).Returns(jobHistoryName);
            _sourceJobRepository.Create(Arg.Any<SourceJobDTO>()).Returns(sourceJobId);

            //ACT
            var sourceJobDto = _instance.CreateSourceJobDto(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, jobHistoryArtifactId, sourceWorkspaceRdoInstanceArtifactId);

            // ASSERT
            ValidateSourceJob(sourceJobDto, sourceJobId, jobHistoryName, jobHistoryArtifactId, sourceWorkspaceRdoInstanceArtifactId);
        }

        [Test]
        public void ItShouldShortenSourceJobName()
        {
            int jobHistoryArtifactId = 806694;
            int sourceWorkspaceRdoInstanceArtifactId = 320154;

            string jobHistoryName = new string('x', 300);
            var sourceJobId = 713321;

            string expectedName = jobHistoryName.Substring(0, 255 - $" - {jobHistoryArtifactId}".Length) + $" - {jobHistoryArtifactId}";

            _jobHistoryRepository.GetJobHistoryName(jobHistoryArtifactId).Returns(jobHistoryName);
            _sourceJobRepository.Create(Arg.Any<SourceJobDTO>()).Returns(sourceJobId);

            //ACT
            var sourceJobDto = _instance.CreateSourceJobDto(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, jobHistoryArtifactId, sourceWorkspaceRdoInstanceArtifactId);

            // ASSERT
            Assert.That(sourceJobDto.Name.Length, Is.EqualTo(Data.Constants.DEFAULT_NAME_FIELD_LENGTH));
            Assert.That(sourceJobDto.Name, Is.EqualTo(expectedName));
        }

        private void ValidateSourceJob(SourceJobDTO sourceJob, int sourceJobArtifactId, string jobHistoryName, int jobHistoryId, int sourceWorkspaceRdoInstanceArtifactId)
        {
            Assert.IsNotNull(sourceJob);
            string expectedName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(jobHistoryName, jobHistoryId);
            Assert.AreEqual(expectedName, sourceJob.Name);
            Assert.AreEqual(sourceWorkspaceRdoInstanceArtifactId, sourceJob.SourceWorkspaceArtifactId);
            Assert.AreEqual(jobHistoryId, sourceJob.JobHistoryArtifactId);
            Assert.AreEqual(jobHistoryName, sourceJob.JobHistoryName);
            Assert.AreEqual(sourceJobArtifactId, sourceJob.ArtifactId);
        }
    }
}