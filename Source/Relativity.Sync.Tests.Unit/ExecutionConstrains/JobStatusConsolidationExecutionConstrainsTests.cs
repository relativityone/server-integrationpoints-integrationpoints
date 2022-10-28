using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    internal class JobStatusConsolidationExecutionConstrainsTests
    {
        [TestCase(false, true)]
        [TestCase(true, false)]
        public async Task CanExecuteAsync_ShouldReturnValue_BasedOnIAPIVersion(bool isIAPIv2, bool shouldExecute)
        {
            // Arrange
            Mock<IAPIv2RunChecker> runChecker = new Mock<IAPIv2RunChecker>();
            runChecker.Setup(x => x.ShouldBeUsed()).Returns(isIAPIv2);

            IExecutionConstrains<IJobStatusConsolidationConfiguration> sut = new JobStatusConsolidationExecutionConstrains(runChecker.Object);

            // Act
            bool result = await sut.CanExecuteAsync(Mock.Of<IJobStatusConsolidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(shouldExecute);
        }
    }
}