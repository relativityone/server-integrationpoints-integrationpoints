namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibraryFactory
	{
		IGenericLibrary<T> Create<T>() where T : BaseRdo, new();
	}
}