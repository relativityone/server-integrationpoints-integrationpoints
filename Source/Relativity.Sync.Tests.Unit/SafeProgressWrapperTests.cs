using Relativity.API;
using System;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SafeProgressWrapperTests
	{
		[Test]
		public void ItShouldPassThru()
		{
			var innerProgress = new Mock<IProgress<SyncJobState>>();
			var instance = new SafeProgressWrapper<SyncJobState>(innerProgress.Object, new EmptyLogger());

			// ACT
			instance.Report(SyncJobState.Start("Test"));

			// ASSERT
			innerProgress.Verify(x => x.Report(It.IsAny<SyncJobState>()));
		}

		[Test]
		public void ItShouldNotThrowWhenWrappedProgressThrows()
		{
			var innerProgress =	new Mock<IProgress<SyncJobState>>();
			innerProgress.Setup(x => x.Report(It.IsAny<SyncJobState>())).Throws<InvalidOperationException>();
			var logger = new Mock<IAPILog>();
			var instance = new SafeProgressWrapper<SyncJobState>(innerProgress.Object, logger.Object);

			// ACT
			instance.Report(SyncJobState.Start("Test"));

			// ASSERT
			logger.Verify(x => x.LogWarning(It.IsAny<InvalidOperationException>(),
				It.Is<string>(s => s.Contains("IProgress")),
				It.IsAny<object[]>()));
		}
	}
}
