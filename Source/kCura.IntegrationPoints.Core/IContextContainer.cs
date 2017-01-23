using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	/// <summary>
	/// Container for all Integration Point context objects
	/// </summary>
	public interface IContextContainer
	{
		/// <summary>
		/// Helper object used for all context creations
		/// </summary>
		IHelper Helper { get; }

		/// <summary>
		/// Services Manager object used for creating service proxies
		/// </summary>
		IServicesMgr ServicesMgr { get; }
	}
}
