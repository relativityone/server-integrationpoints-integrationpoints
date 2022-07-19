using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class SourceJobTagServiceTests
    {
        private Mock<IRelativitySourceJobTagRepository> _sourceJobTagRepository;
        private Mock<IJobHistoryNameQuery> _jobHistoryNameQuery;
        private Mock<ITagNameFormatter> _tagNameFormatter;

        private SourceJobTagService _sut;

        [SetUp]
        public void SetUp()
        {
            _sourceJobTagRepository = new Mock<IRelativitySourceJobTagRepository>();
            _jobHistoryNameQuery = new Mock<IJobHistoryNameQuery>();
            _tagNameFormatter = new Mock<ITagNameFormatter>();

            _sut = new SourceJobTagService(_sourceJobTagRepository.Object, _jobHistoryNameQuery.Object, _tagNameFormatter.Object);
        }

        [Test]
        public async Task ItShouldCreateSourceJobTag()
        {
            const string jobHistoryName= "job name";
            const string jobTagName = "tag name";
            _jobHistoryNameQuery.Setup(x => x.GetJobNameAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(jobHistoryName);
            _tagNameFormatter.Setup(x => x.FormatSourceJobTagName(It.IsAny<string>(), It.IsAny<int>())).Returns(jobTagName);

            // act
            await _sut.CreateOrReadSourceJobTagAsync(Mock.Of<IDestinationWorkspaceTagsCreationConfiguration>(), 0, CancellationToken.None).ConfigureAwait(false);

            // assert
            _sourceJobTagRepository.Verify(x => x.CreateAsync(It.IsAny<int>(), It.Is<RelativitySourceJobTag>(tag => 
                tag.Name.Equals(jobTagName, StringComparison.InvariantCulture) && tag.JobHistoryName.Equals(jobHistoryName, StringComparison.InvariantCulture)), CancellationToken.None));
        }
    }
}