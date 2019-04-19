using System;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SyncJobExecutionConfigurationTests
	{
		private SyncJobExecutionConfiguration _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new SyncJobExecutionConfiguration();
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(-100)]
		public void ItShouldNotAllowBatchSizeToBeLessThanOne(int batchSize)
		{
			Action action = () => _instance.BatchSize = batchSize;

			action.Should().Throw<ArgumentException>();
		}

		[Test]
		[TestCase(1)]
		[TestCase(100)]
		[TestCase(1000)]
		public void ItShouldAllowBatchSizeToBeGreaterThanZero(int batchSize)
		{
			_instance.BatchSize = batchSize;

			_instance.BatchSize.Should().Be(batchSize);
		}
	}
}