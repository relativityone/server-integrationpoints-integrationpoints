using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class TagNameFormatterTests
    {
        private TagNameFormatter _sut;
        private const int _MAX_LENGTH = 255;

        [SetUp]
        public void SetUp()
        {
            _sut = new TagNameFormatter(new EmptyLogger());
        }

        [Test]
        public void ItShouldShortenTagName()
        {
            const string destinationInstanceName = "instance";
            const int destinationWorkspaceArtifactId = 3;
            const string tooLongDestinationWorkspaceName =
                "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
                "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLo";

            // act
            string name = _sut.FormatWorkspaceDestinationTagName(
                destinationInstanceName,
                tooLongDestinationWorkspaceName, destinationWorkspaceArtifactId);

            // assert
            Assert.LessOrEqual(name.Length, _MAX_LENGTH);
        }

        [Test]
        public void ItShouldShortenSourceJobTagName()
        {
            string tooLongJobHistoryName =
                "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
                "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
                "TooLongNameTooLongNameTooLongNameTooLongNameTooLongName";
            int jobHistoryArtifactId = 1;

            // act
            string name = _sut.FormatSourceJobTagName(tooLongJobHistoryName, jobHistoryArtifactId);

            // assert
            Assert.LessOrEqual(name.Length, _MAX_LENGTH);
        }

        [Test]
        public void ItShouldShortenSourceCaseTagName()
        {
            const string tooLongInstanceName = "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
                    "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
                    "TooLongNameTooLongNameTooLongNameTooLongNameTooLongName";
            string workspaceName = "workspace";
            int workspaceArtifactId = 1;

            // act
            string name = _sut.FormatSourceCaseTagName(tooLongInstanceName, workspaceName, workspaceArtifactId);

            // assert
            Assert.LessOrEqual(name.Length, _MAX_LENGTH);
        }
    }
}
