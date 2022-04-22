using Relativity.API;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
	internal class NonAdminCanSyncUsingLinks : INonAdminCanSyncUsingLinks
	{
		private bool? _isEnabled;
		
		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _logger;

		public NonAdminCanSyncUsingLinks(IToggleProvider toggleProvider, IAPILog logger)
		{
			_toggleProvider = toggleProvider;
			_logger = logger;
		}
		
		public bool IsEnabled()
		{
			if (_isEnabled != null)
			{
				return (bool)_isEnabled;
			}

			_isEnabled = _toggleProvider.IsEnabled<EnableNonAdminSyncLinksToggle>();
			_logger.LogInformation("Toggle EnableNonAdminSyncLinksToggle: {toggleValue}", _isEnabled);
			return (bool)_isEnabled;
		}
	}
}