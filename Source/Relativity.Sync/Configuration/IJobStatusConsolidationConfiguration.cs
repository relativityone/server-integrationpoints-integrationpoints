using System;

namespace Relativity.Sync.Configuration
{
	internal interface IJobStatusConsolidationConfiguration : IConfiguration
	{
		Guid ExportRunId { get; }

		int TotalRecordsCount { get; }
	}
}