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
	public class JobStartMetricsExecutorConstrainsTests
	{
		[Test]
		public async Task CanExecuteAsyncShouldReturnTrueTest()
		{
			// Arrange
			var instance = new JobStartMetricsExecutorConstrains();

			// Act
			bool actualResult = await instance.CanExecuteAsync(Mock.Of<ISumReporterConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Should().BeTrue();
		}
	}
}