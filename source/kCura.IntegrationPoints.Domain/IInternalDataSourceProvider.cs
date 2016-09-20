using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain
{
	/// <summary>
	/// Represents internal data source provider of which can inject its dependencies
	/// </summary>
	public interface IInternalDataSourceProvider : IDataSourceProvider
	{
		/// <summary>
		/// Register additional dependencies that the source provider needs to operate.
		/// </summary>
		/// <typeparam name="T">A type of the dependency</typeparam>
		/// <param name="dependency">A dependency object to be used in the source provider. The dependency must be marshallable.</param>
		void RegisterDependency<T>(T dependency);
	}
}