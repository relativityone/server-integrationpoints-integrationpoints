using System;
using kCura.Vendor.Castle.Core.Internal;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class PipelineSelectorConfiguration : IPipelineSelectorConfiguration, IDisposable
	{
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");
		private static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");


		private readonly IConfiguration _cache;

		public PipelineSelectorConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)?.ArtifactID;
		public bool IsImageJob => !string.IsNullOrEmpty(_cache.GetFieldValue<string>(ImageFileCopyModeGuid));

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
