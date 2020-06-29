using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration
	{
		private static readonly Guid JobHistoryToRetryArtifactIdGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

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
