using System;
using System.Security.Claims;
using Relativity.Core;
using Relativity.Core.Authentication;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class BaseServiceContextProvider : IBaseServiceContextProvider
	{
		private readonly ClaimsPrincipal _claimsPrincipal;

		public BaseServiceContextProvider(ClaimsPrincipal claimsPrincipal)
		{
			_claimsPrincipal = claimsPrincipal;
		}

		public BaseServiceContext GetUnversionContext(int workspaceArtifactId)
		{
			try
			{
				return _claimsPrincipal.GetServiceContextUnversionShortTerm(workspaceArtifactId);
			}
			catch (Exception exception)
			{
				throw new Exception("Unable to initialize the user context.", exception);
			}
		}
	}
}
