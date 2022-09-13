using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Pipelines
{
    internal class IAPIv2RunChecker : IIAPIv2RunChecker
    {
        private bool? _shouldBeUsed;

        private readonly ISyncToggles _toggles;
        private readonly IIAPIv2RunCheckerConfiguration _configuration;

        public IAPIv2RunChecker(IIAPIv2RunCheckerConfiguration configuration, ISyncToggles toggles)
        {
            _toggles = toggles;
            _configuration = configuration;
        }

        public bool? ShouldBeUsed()
        {
            if (_shouldBeUsed == null)
            {
                CheckConditions();
            }

            return _shouldBeUsed;
        }

        private void CheckConditions()
        {
            _shouldBeUsed = _toggles.IsEnabled<EnableIAPIv2Toggle>()
                && _configuration.RdoArtifactTypeId == (int)ArtifactType.Document
                && (_configuration.NativeBehavior == ImportNativeFileCopyMode.SetFileLinks || _configuration.NativeBehavior == ImportNativeFileCopyMode.DoNotImportNativeFiles)
                && !_configuration.IsRetried
                && !_configuration.IsDrainStopped
                && !_configuration.HasLongTextFields
                && !_configuration.ImageImport;
        }
    }
}
