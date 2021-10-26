using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration, IDisposable
	{
		private readonly IConfiguration _cache;

		public PipelineSelectorConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public bool IsImageJob => _cache.GetFieldValue(x => x.ImageImport);

		public int RdoArtifactTypeId => _cache.GetFieldValue(x => x.RdoArtifactTypeId);

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
