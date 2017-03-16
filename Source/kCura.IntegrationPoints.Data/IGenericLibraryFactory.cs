using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibraryFactory
	{
		IGenericLibrary<T> Create<T>(ExecutionIdentity executionIdentity) where T : BaseRdo, new();
	}
}