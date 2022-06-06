using Relativity.Toggles;

namespace Relativity.Sync.Toggles.Service
{
	internal interface ISyncToggles
	{
        bool IsEnabled<T>() where T : IToggle;
    }
}