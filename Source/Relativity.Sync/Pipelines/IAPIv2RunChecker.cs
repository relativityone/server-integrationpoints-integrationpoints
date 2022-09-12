using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Pipelines
{
    internal class IAPIv2RunChecker : IIAPIv2RunChecker
    {
        private static bool? _shouldBeUsed;

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
            // Check Toggle -Relativity.Sync.Toggles.EnableIAPIv2Toggle
            // IAPI 2.0 should be used only Documents flow
            // IAPI 2.0 should be used only for jobs with DoNotCopyFiles or SetLinks
            // IAPI 2.0 should not be used for jobs where Long Text fields are involved
            // IAPI 2.0 should not be used for Import Images flow
            // Job should not be retried / drain - stopped

            _shouldBeUsed = _toggles.IsEnabled<EnableIAPIv2Toggle>()
                && _configuration.RdoArtifactTypeId == (int)ArtifactType.Document
                && (_configuration.NativeBehavior == ImportNativeFileCopyMode.SetFileLinks || _configuration.NativeBehavior == ImportNativeFileCopyMode.DoNotImportNativeFiles)
                && _configuration.IsRetried == false
                && _configuration.HasLongTextFields == false
                && _configuration.ImageImport == false;
        }
    }
}
