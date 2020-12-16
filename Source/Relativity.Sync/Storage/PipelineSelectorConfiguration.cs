using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration, IDisposable
	{
		private static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");


		private readonly IConfiguration _cache;

		public PipelineSelectorConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryGuid)?.ArtifactID;

		public bool IsImageJob => _cache.GetFieldValue<bool>(ImageImportGuid);

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
