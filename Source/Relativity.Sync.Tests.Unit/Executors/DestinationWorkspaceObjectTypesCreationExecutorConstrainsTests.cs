using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class DestinationWorkspaceObjectTypesCreationExecutorConstrainsTests
    {
        private DestinationWorkspaceObjectTypesCreationExecutorConstrains _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new DestinationWorkspaceObjectTypesCreationExecutorConstrains();
        }

        [Test]
        public async Task ItShouldAlwaysAllowExecution()
        {
            // act
            bool canExecute = await _instance.CanExecuteAsync(Mock.Of<IDestinationWorkspaceObjectTypesCreationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // assert
            canExecute.Should().BeTrue();
        }
    }
}