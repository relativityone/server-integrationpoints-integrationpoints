using NUnit.Framework;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class RelativitySourceCaseTagTests
    {
        private RelativitySourceCaseTag _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new RelativitySourceCaseTag();
        }

        [Test]
        public void ItShouldNotRequireUpdate()
        {
            const string tagName = "tag name";
            const string sourceInstanceName = "source instance";
            const string sourceWorkspaceName = "source workspace";

            _sut.Name = tagName;
            _sut.SourceInstanceName = sourceInstanceName;
            _sut.SourceWorkspaceName = sourceWorkspaceName;

            // act
            bool requiresUpdate = _sut.RequiresUpdate(tagName, sourceInstanceName, sourceWorkspaceName);

            // assert
            Assert.False(requiresUpdate);
        }

        [Test]
        public void ItShouldNotRequireUpdateWhenTagNameIsOutdated()
        {
            const string tagName = "tag name";
            const string sourceInstanceName = "source instance";
            const string sourceWorkspaceName = "source workspace";

            _sut.Name = tagName;
            _sut.SourceInstanceName = sourceInstanceName;
            _sut.SourceWorkspaceName = sourceWorkspaceName;

            const string newTagName = "new tag name";

            // act
            bool requiresUpdate = _sut.RequiresUpdate(newTagName, sourceInstanceName, sourceWorkspaceName);

            // assert
            Assert.True(requiresUpdate);
        }

        [Test]
        public void ItShouldNotRequireUpdateWhenSourceInstanceNameIsOutdated()
        {
            const string tagName = "tag name";
            const string sourceInstanceName = "source instance";
            const string sourceWorkspaceName = "source workspace";

            _sut.Name = tagName;
            _sut.SourceInstanceName = sourceInstanceName;
            _sut.SourceWorkspaceName = sourceWorkspaceName;

            const string newSourceInstanceName = "new source instance";

            // act
            bool requiresUpdate = _sut.RequiresUpdate(tagName, newSourceInstanceName, sourceWorkspaceName);

            // assert
            Assert.True(requiresUpdate);
        }

        [Test]
        public void ItShouldNotRequireUpdateWhenSourceWorkspaceNameIsOutdated()
        {
            const string tagName = "tag name";
            const string sourceInstanceName = "source instance";
            const string sourceWorkspaceName = "source workspace";

            _sut.Name = tagName;
            _sut.SourceInstanceName = sourceInstanceName;
            _sut.SourceWorkspaceName = sourceWorkspaceName;

            const string newSourceWorkspaceName = "new source workspace";

            // act
            bool requiresUpdate = _sut.RequiresUpdate(tagName, sourceInstanceName, newSourceWorkspaceName);

            // assert
            Assert.True(requiresUpdate);
        }
    }
}
