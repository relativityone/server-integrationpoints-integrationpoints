using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class IAPIv2RunCheckerConfiguration : IIAPIv2RunCheckerConfiguration
    {
        private readonly IConfiguration _cache;

        public IAPIv2RunCheckerConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public ImportNativeFileCopyMode NativeBehavior => _cache.GetFieldValue(x => x.NativesBehavior);

        public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

        public int RdoArtifactTypeId => _cache.GetFieldValue(x => x.RdoArtifactTypeId);

        public bool IsRetried => _cache.GetFieldValue(x => x.JobHistoryToRetryId).HasValue && _cache.GetFieldValue(x => x.JobHistoryToRetryId) > 0;

        // TODO: 
        // how to implement DrainStopped and LongTextInvolving flags?
        public bool IsDrainStopped => throw new NotImplementedException();

        public bool HasLongTextFields => throw new NotImplementedException();
    }
}
