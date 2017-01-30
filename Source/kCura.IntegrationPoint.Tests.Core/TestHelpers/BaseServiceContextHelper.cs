using Relativity.Core;
using Relativity.Core.Service;
using Context = kCura.Data.RowDataGateway.Context;
using SystemArtifactQuery = Relativity.Data.SystemArtifactQuery;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class BaseServiceContextHelper
	{
		public static BaseServiceContext Create()
		{
			var loginManager = new LoginManager();
			var userID = SystemArtifactQuery.Instance.RetrieveArtifactIDByIdentifier(new Context(), "ServiceAccountArtifactID");
			var identity = loginManager.GetLoginIdentity(userID);
			return new ServiceContext(identity, "IntegrationTests", -1);
		}
	}
}