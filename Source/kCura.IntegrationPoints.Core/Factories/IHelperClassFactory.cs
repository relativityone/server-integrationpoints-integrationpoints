using kCura.IntegrationPoints.Core.Helpers;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IHelperClassFactory
    {
        /// <summary>
        /// Returns an instance of an OnClickEventHelper object.
        /// </summary>
        /// <param name="managerFactory">A factory used to create manager objects.</param>
        /// <returns>An instance of an OnClickEventHelper object</returns>
        IOnClickEventConstructor CreateOnClickEventHelper(IManagerFactory managerFactory);
    }
}
