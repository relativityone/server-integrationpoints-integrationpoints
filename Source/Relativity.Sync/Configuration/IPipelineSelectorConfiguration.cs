using System;

namespace Relativity.Sync.Configuration
{
	internal interface IPipelineSelectorConfiguration : IDisposable
	{
		int? JobHistoryToRetryId { get; }
	}
}