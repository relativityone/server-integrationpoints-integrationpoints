using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	/// <summary>
	/// Container for all Integration Point context objects
	/// </summary>
	public interface IContextContainer
	{
		IHelper Helper { get; }
	}
}
