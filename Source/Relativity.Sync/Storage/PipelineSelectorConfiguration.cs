using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration
	{
		private static readonly Guid JobHistoryToRetryArtifactIdGuid = new Guid("2fee1c74-7e5b-4034-9721-984b0b9c1fef");

		private readonly IConfiguration _cache;

		public PipelineSelectorConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<int?>(JobHistoryToRetryArtifactIdGuid);

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
