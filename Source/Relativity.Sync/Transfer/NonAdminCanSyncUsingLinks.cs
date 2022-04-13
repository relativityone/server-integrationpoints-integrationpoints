using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
	internal class NonAdminCanSyncUsingLinks : INonAdminCanSyncUsingLinks
	{
		private bool? _isEnabled;
		
		private readonly IToggleProvider _toggleProvider;
		private readonly ISyncLog _logger;
		private readonly IDocumentSynchronizationConfiguration _synchronizationConfiguration;

		public NonAdminCanSyncUsingLinks(IToggleProvider toggleProvider, ISyncLog logger, IDocumentSynchronizationConfiguration synchronizationConfiguration)
		{
			_toggleProvider = toggleProvider;
			_logger = logger;
			_synchronizationConfiguration = synchronizationConfiguration;
		}
		
		public bool IsEnabled()
		{
			if (_isEnabled != null)
			{
				return (bool)_isEnabled;
			}

			bool toggleValue = _toggleProvider.IsEnabled<EnableNonAdminSyncLinksToggle>();
			bool isCopyModeFileLinks = _synchronizationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.SetFileLinks;
			_logger.LogInformation("Validating if NonAdmin user can sync using Links options. Toggle EnableNonAdminSyncLinksToggle: {toggleValue}, CopyMode: {ImportNativeFileCopyMode}", toggleValue, _synchronizationConfiguration.ImportNativeFileCopyMode);
			if (toggleValue && isCopyModeFileLinks)
			{
				_isEnabled = true;
			}
			
			return (bool)_isEnabled;
		}
	}
}