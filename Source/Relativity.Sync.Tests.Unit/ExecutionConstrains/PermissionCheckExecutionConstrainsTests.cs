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
    public class PermissionCheckExecutionConstrainsTests
    {
        private PermissionCheckExecutionConstrains _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new PermissionCheckExecutionConstrains();
        }

        [Test]
        public async Task ItShouldAlwaysCanExecute()
        {
            //Arrange
            var configuration = new Mock<IPermissionsCheckConfiguration>();

            //Act
            bool actualResult = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            //Assert
            actualResult.Should().BeTrue();
        }
    }
}
