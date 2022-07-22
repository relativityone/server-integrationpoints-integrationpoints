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
    [Parallelizable(ParallelScope.All)]
    public class NotificationExecutionConstrainsTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task CanExecuteAsyncTests(bool sendEmailsConfiguration)
        {
            // Arrange
            var configuration = new Mock<INotificationConfiguration>();
            configuration.SetupGet(x => x.SendEmails).Returns(sendEmailsConfiguration);

            var instance = new NotificationExecutionConstrains();

            // Act
            bool actualResult = await instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            actualResult.Should().Be(sendEmailsConfiguration);
        }
    }
}