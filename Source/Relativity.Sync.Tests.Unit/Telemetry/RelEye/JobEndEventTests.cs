using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry.RelEye;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Telemetry.RelEye
{
    internal class JobEndEventTests
    {
        private IFixture _fxt = FixtureFactory.Create();

        [Test]
        public void GetValues_ShouldReturnAttributes()
        {
            // Arrange
            var @event = _fxt.Build<JobEndEvent>()
                .With(x => x.Status, ExecutionStatus.Completed)
                .Create();

            // Act
            Dictionary<string, object> attributes = @event.GetValues();

            // Assert
            attributes[Const.Names.JobResult].Should().Be(@event.Status.ToString());
        }
    }
}
