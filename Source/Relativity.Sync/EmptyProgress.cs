using System;
using System.Diagnostics.CodeAnalysis;

namespace Relativity.Sync
{
	[ExcludeFromCodeCoverage]
	internal sealed class EmptyProgress : IProgress<SyncProgress>
	{
		public void Report(SyncProgress value)
		{
			// Method intentionally left empty.
		}
	}
}