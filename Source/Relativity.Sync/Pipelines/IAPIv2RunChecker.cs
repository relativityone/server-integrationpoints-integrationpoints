using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Pipelines
{
    internal class IAPIv2RunChecker : IIAPIv2RunChecker
    {
        private readonly ISyncToggles _toggles;
        private readonly IIAPIv2RunCheckerConfiguration _configuration;
        private readonly IFieldMappings _fieldMappings;
        private readonly IObjectFieldTypeRepository _objectFieldTypeRepository;
        private readonly IAPILog _logger;

        private bool? _shouldBeUsed;

        public IAPIv2RunChecker(
            IIAPIv2RunCheckerConfiguration configuration,
            ISyncToggles toggles,
            IFieldMappings fieldMappings,
            IObjectFieldTypeRepository objectFieldTypeRepository,
            IAPILog logger)
        {
            _toggles = toggles;
            _configuration = configuration;
            _fieldMappings = fieldMappings;
            _objectFieldTypeRepository = objectFieldTypeRepository;
            _logger = logger;
        }

        public bool ShouldBeUsed()
        {
            if (!_shouldBeUsed.HasValue)
            {
                _shouldBeUsed = CheckConditionsAsync().GetAwaiter().GetResult();
            }

            return _shouldBeUsed.Value;
        }

        private async Task<bool> CheckConditionsAsync()
        {
            try
            {
                return _toggles.IsEnabled<EnableIAPIv2Toggle>()
                       && _configuration.RdoArtifactTypeId == (int)ArtifactType.Document
                       && (_configuration.NativeBehavior == ImportNativeFileCopyMode.SetFileLinks || _configuration.NativeBehavior == ImportNativeFileCopyMode.DoNotImportNativeFiles)
                       && !_configuration.IsRetried
                       && !_configuration.IsDrainStopped
                       && !_configuration.ImageImport
                       && !await LongTextFieldsMapped().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking if IAPI 2.0 should be used");
                throw;
            }
        }

        private async Task<bool> LongTextFieldsMapped()
        {
            IList<FieldMap> mappedFields = _fieldMappings.GetFieldMappings();
            ICollection<string> fieldNames = mappedFields.Select(x => x.SourceField.DisplayName).ToArray();

            IDictionary<string, RelativityDataType> fieldDataTypes = await _objectFieldTypeRepository.GetRelativityDataTypesForFieldsByFieldNameAsync(
                _configuration.SourceWorkspaceArtifactId,
                _configuration.RdoArtifactTypeId,
                fieldNames,
                CancellationToken.None)
             .ConfigureAwait(false);

            return fieldDataTypes.Any(x => x.Value == RelativityDataType.LongText);
        }
    }
}
