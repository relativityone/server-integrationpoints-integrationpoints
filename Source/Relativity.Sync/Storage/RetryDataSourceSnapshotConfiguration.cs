using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class RetryDataSourceSnapshotConfiguration : DataSourceSnapshotConfigurationBase, IRetryDataSourceSnapshotConfiguration
	{
		private static readonly Guid JobHistoryToRetryArtifactIdGuid = new Guid("2fee1c74-7e5b-4034-9721-984b0b9c1fef");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");


		public int? JobHistoryToRetryId => _cache.GetFieldValue<int?>(JobHistoryToRetryArtifactIdGuid);
		public ImportOverwriteMode ImportOverwriteMode
		{
			get => (ImportOverwriteMode) (Enum.Parse(typeof(ImportOverwriteMode),
				_cache.GetFieldValue<string>(ImportOverwriteModeGuid)));
			set => _cache.UpdateFieldValueAsync(ImportOverwriteModeGuid, value.ToString());
		}


		public RetryDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters) : base(cache, fieldMappings, syncJobParameters)
		{
		}
	}
}
