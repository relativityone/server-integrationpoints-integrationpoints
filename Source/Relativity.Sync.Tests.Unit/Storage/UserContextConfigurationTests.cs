using System;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    public class UserContextConfigurationTests
    {
        [Test]
        public void UserId_ShouldReturnProperUserId()
        {
            // Arrange
            const int userId = 3;
            SyncJobParameters syncJobParameters = new SyncJobParameters(0, 1, userId, Guid.Empty);

            // Act
            int actualUserId = syncJobParameters.UserId;

            // Assert
            actualUserId.Should().Be(userId);
        }
    }
}