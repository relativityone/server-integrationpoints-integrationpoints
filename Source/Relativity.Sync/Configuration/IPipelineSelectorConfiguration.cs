using System;

namespace Relativity.Sync.Configuration
{
	internal interface IPipelineSelectorConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }
	}
}