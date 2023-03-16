using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry.RelEye;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Telemetry.RelEye
{
    internal class JobStartEventTests
    {
        private IFixture _fxt = FixtureFactory.Create();

        [Test]
        public void GetValues_ShouldReturnAttributes()
        {
            // Arrange
            var @event = _fxt.Create<JobStartEvent>();

            // Act
            Dictionary<string, object> attributes = @event.GetValues();

            // Assert
            attributes[Const.Names.JobType].Should().Be(@event.Type);
        }
    }
}
