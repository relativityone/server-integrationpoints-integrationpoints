using System;
using System.Security.Claims;
using Relativity.Core;
using Relativity.Core.Authentication;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class ClaimsPrincipalExtension
	{
		public static BaseServiceContext GetUnversionContext(this ClaimsPrincipal claimsPrincipal, int workspaceArtifactId)
		{
			try
			{
				return claimsPrincipal.GetServiceContextUnversionShortTerm(workspaceArtifactId);
			}
			catch (Exception exception)
			{
				throw new Exception("Unable to initialize the user context.", exception);
			}
		}
	}
}