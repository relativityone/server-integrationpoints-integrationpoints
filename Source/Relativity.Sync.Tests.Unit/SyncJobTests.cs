using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncJobTests
	{
		private SyncJob _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new SyncJob();
		}

		[Test]
		public void ItShouldExecuteJob()
		{
			Func<Task> action = async () => await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<NotImplementedException>();
		}

		[Test]
		public void ItShouldRetryJob()
		{
			Func<Task> action = async () => await _instance.RetryAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<NotImplementedException>();
		}
	}
}