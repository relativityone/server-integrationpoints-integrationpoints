using System;

namespace Relativity.Sync.Configuration
{
	internal interface IJobCleanupConfiguration : IConfiguration
	{
		Guid ExportRunId { get; }

		int TotalRecordsCount { get; }
	}
}