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
    public sealed class PreValidationExecutionConstrainsTests
    {
        private PreValidationExecutionConstrains _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new PreValidationExecutionConstrains();
        }

        [Test]
        public async Task ItShouldAlwaysCanExecute()
        {
            // Act
            bool canExecute = await _sut.CanExecuteAsync(Mock.Of<IPreValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            canExecute.Should().BeTrue();
        }
    }
}
