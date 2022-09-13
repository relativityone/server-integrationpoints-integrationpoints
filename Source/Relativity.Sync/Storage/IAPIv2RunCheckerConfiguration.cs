using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Storage
{
    internal class IAPIv2RunCheckerConfiguration : IIAPIv2RunCheckerConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly IFieldManager _fieldManager;

        public IAPIv2RunCheckerConfiguration(IConfiguration cache, IFieldManager fieldManager)
        {
            _cache = cache;
            _fieldManager = fieldManager;
        }

        public ImportNativeFileCopyMode NativeBehavior => _cache.GetFieldValue(x => x.NativesBehavior);

        public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

        public int RdoArtifactTypeId => _cache.GetFieldValue(x => x.RdoArtifactTypeId);

        public bool IsRetried => _cache.GetFieldValue(x => x.JobHistoryToRetryId).HasValue && _cache.GetFieldValue(x => x.JobHistoryToRetryId) > 0;

        public bool IsDrainStopped => _cache.GetFieldValue(x => x.Resuming);

        public bool HasLongTextFields => LongTextFieldsMapped().GetAwaiter().GetResult();

        private async Task<bool> LongTextFieldsMapped()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            IList<FieldInfoDto> fieldInfo = await _fieldManager.GetMappedFieldsAsync(token).ConfigureAwait(false);

            return fieldInfo.Any(x => x.RelativityDataType == RelativityDataType.LongText);
        }
    }
}
