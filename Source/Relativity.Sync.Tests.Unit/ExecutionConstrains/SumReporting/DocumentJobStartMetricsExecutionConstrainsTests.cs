using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains.SumReporting;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains.SumReporting
{
	[TestFixture]
	internal class DocumentJobStartMetricsExecutionConstrainsTests
	{
		[Test]
		public async Task CanExecuteAsyncShouldReturnTrueTest()
		{
			// Arrange
			var sut = new DocumentJobStartMetricsExecutorConstrains();

			// Act
			bool actualResult = await sut.CanExecuteAsync(Mock.Of<IDocumentJobStartMetricsConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Should().BeTrue();
		}
    }
}
