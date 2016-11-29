using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class JobHistoryLibraryFactory : IJobHistoryLibraryFactory
	{
		private readonly IHelper _helper;

		public JobHistoryLibraryFactory(IHelper helper)
		{
			_helper = helper;
		}

		public IGenericLibrary<Data.JobHistory> Create(int workspaceId)
		{
			return new RsapiClientLibrary<Data.JobHistory>(_helper, workspaceId);
		}
	}
}