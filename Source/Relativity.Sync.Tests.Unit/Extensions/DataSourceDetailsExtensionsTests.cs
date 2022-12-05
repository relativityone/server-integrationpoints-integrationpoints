using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Import.V1.Models.Sources;
using Relativity.Sync.Extensions;

namespace Relativity.Sync.Tests.Unit.Extensions
{
    internal class DataSourceDetailsExtensionsTests
    {
        private IFixture _fxt;

        [SetUp]
        public void SetUp()
        {
            _fxt = new Fixture();
        }

        [Test]
        public void IsFinished_ShouldBeTrue_WhenDataSourceEndState([Values] DataSourceState state)
        {
            // Arrange
            DataSourceDetails dataSource = _fxt.Build<DataSourceDetails>()
                .With(x => x.State, state)
                .Create();

            bool expectedResult = ExpectedResult(state);

            // Act
            bool result = dataSource.IsFinished();

            // Assert
            result.Should().Be(expectedResult);
        }

        private bool ExpectedResult(DataSourceState state)
        {
            switch (state)
            {
                case DataSourceState.Canceled:
                case DataSourceState.Failed:
                case DataSourceState.CompletedWithItemErrors:
                case DataSourceState.Completed:
                    return true;
                default:
                    return false;
            }
        }
    }
}
