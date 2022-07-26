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
    public sealed class DestinationWorkspaceTagsCreationExecutionConstrainsTests
    {
        [Test]
        public async Task ItShouldAlwaysReturnTrue()
        {
            DestinationWorkspaceTagsCreationExecutionConstrains instance = new DestinationWorkspaceTagsCreationExecutionConstrains();

            bool result = await instance.CanExecuteAsync(Mock.Of<IDestinationWorkspaceTagsCreationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            result.Should().BeTrue();
        }
    }
}
