using Relativity.API;
using Relativity.Toggles;

namespace Relativity.Sync.Toggles.Service
{
	internal class SyncToggles : ISyncToggles
	{
		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _logger;

		public SyncToggles(IToggleProvider toggleProvider, IAPILog logger)
		{
			_toggleProvider = toggleProvider;
			_logger = logger;
		}
		
        public bool IsEnabled<T>() where T: IToggle
        {
            bool isEnabled = _toggleProvider.IsEnabled<T>();
			_logger.LogInformation("Toggle {toggleName} is enabled: {isEnabled}", nameof(T), isEnabled);
            return isEnabled;
        }
    }
}