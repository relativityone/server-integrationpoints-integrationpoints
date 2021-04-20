using System;

namespace Relativity.Sync.Tests.Common
{
	public static class FakeHelper
	{
		public static SyncJobParameters CreateSyncJobParameters()
		{
			return new SyncJobParameters(1, 1, Guid.NewGuid());
		}
	}
}
