using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	/// <summary>
	/// Factory responsible for context container creation.
	/// </summary>
	public interface IContextContainerFactory
	{
		/// <summary>
		/// Creates context container
		/// </summary>
		/// <param name="helper">Helper object</param>
		/// <returns>Context container</returns>
		IContextContainer CreateContextContainer(IHelper helper);

		/// <summary>
		/// Creates context container
		/// </summary>
		/// <param name="helper">Helper object used to create necessary contexts</param>
		/// <param name="servicesMgr">Services Manager object used for creating service proxies</param>
		/// <returns>Context container</returns>
		IContextContainer CreateContextContainer(IHelper helper, IServicesMgr servicesMgr);
	}
}
