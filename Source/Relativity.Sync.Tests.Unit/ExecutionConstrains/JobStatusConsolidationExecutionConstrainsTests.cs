using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    internal class JobStatusConsolidationExecutionConstrainsTests
    {
        [Test]
        public async Task CanExecuteAsync_ShouldAlwaysReturnTrue()
        {
            // Arrange
            IExecutionConstrains<IJobStatusConsolidationConfiguration> sut = new JobStatusConsolidationExecutionConstrains();

            // Act
            bool result = await sut.CanExecuteAsync(Mock.Of<IJobStatusConsolidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }
    }
}