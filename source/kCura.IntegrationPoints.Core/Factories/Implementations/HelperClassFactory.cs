using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class HelperClassFactory : IHelperClassFactory
	{
		public IOnClickEventHelper CreateOnClickEventHelper(IManagerFactory managerFactory, IContextContainer contextContainer)
		{
			return new OnClickEventHelper(contextContainer, managerFactory);
		}
	}
}
