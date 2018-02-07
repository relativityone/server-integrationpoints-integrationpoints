using kCura.IntegrationPoints.Email;

namespace kCura.IntegrationPoints.Agent
{
	/// <summary>
	/// Responsible for creating Relativity configurations.
	/// </summary>
	public interface IRelativityConfigurationFactory
	{
		/// <summary>
		/// Retrieves an <see cref="EmailConfiguration"/> instance.
		/// </summary>
		/// <returns>An instance of the <see cref="EmailConfiguration"/> class.</returns>
		EmailConfiguration GetConfiguration();
	}
}