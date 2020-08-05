using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration, IDisposable
	{
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

		private readonly IConfiguration _cache;

		public PipelineSelectorConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)?.ArtifactID;

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
