using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class HelperClassFactory : IHelperClassFactory
    {
        public IOnClickEventConstructor CreateOnClickEventHelper(IManagerFactory managerFactory)
        {
            return new OnClickEventConstructor(managerFactory);
        }
    }
}
