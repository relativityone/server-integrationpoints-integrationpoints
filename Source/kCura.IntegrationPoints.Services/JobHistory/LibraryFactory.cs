using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class LibraryFactory : ILibraryFactory
	{
		private readonly IHelper _helper;

		public LibraryFactory(IHelper helper)
		{
			_helper = helper;
		}

		public IGenericLibrary<T> Create<T>(int workspaceId) where T : BaseRdo, new()
		{
			return new RsapiClientLibrary<T>(_helper, workspaceId);
		}
	}
}