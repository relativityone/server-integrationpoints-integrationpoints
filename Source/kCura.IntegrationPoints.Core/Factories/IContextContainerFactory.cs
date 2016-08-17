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
		/// <param name="helper">Helper object used to create necessary contexts</param>
		/// <returns>Context container</returns>
		IContextContainer CreateContextContainer(IHelper helper);
	}
}
