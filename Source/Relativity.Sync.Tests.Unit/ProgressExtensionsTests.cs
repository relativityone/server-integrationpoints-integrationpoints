using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    internal sealed class ProgressExtensionsTests
    {
        [Test]
        public void ItShouldThrowWhenArrayIsNull()
        {
            IProgress<SyncJobState>[] progressReporters = null;

            // ACT
            Action action = () => progressReporters.Combine();

            // ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ItShouldReturnEmptyProgressWhenArrayIsEmpty()
        {
            IProgress<SyncJobState>[] progressReporters = Array.Empty<IProgress<SyncJobState>>();

            // ACT
            IProgress<SyncJobState> result = progressReporters.Combine();

            // ASSERT
            Assert.IsInstanceOf<EmptyProgress<SyncJobState>>(result);
        }

        [Test]
        public void ItShouldReturnFirstProcessWhenArrayHasSingleElement()
        {
            Progress<SyncJobState> expected = new Progress<SyncJobState>();
            IProgress<SyncJobState>[] progressReporters = { expected };

            // ACT
            IProgress<SyncJobState> result = progressReporters.Combine();

            // ASSERT
            Assert.AreSame(expected, result);

        }

        [Test]
        public void ItShouldReturnCombinedProcessWhenArrayHasManyElements()
        {
            var progress1 = new Mock<IProgress<SyncJobState>>();
            var progress2 = new Mock<IProgress<SyncJobState>>();
            var progress3 = new Mock<IProgress<SyncJobState>>();
            IProgress<SyncJobState>[] progressReporters = { progress1.Object, progress2.Object, progress3.Object };

            // ACT
            IProgress<SyncJobState> result = progressReporters.Combine();
            SyncJobState value = SyncJobState.Start("Test");
            result.Report(value);

            // ASSERT
            progress1.Verify(x => x.Report(It.Is<SyncJobState>(v => v.Id == "Test")));
            progress2.Verify(x => x.Report(It.Is<SyncJobState>(v => v.Id == "Test")));
            progress3.Verify(x => x.Report(It.Is<SyncJobState>(v => v.Id == "Test")));
        }
    }
}
