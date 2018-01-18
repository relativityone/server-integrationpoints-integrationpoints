using System;
using System.Security.Claims;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Data;

namespace kCura.IntegrationPoints.Data.Extensions
{
    public static class ClaimsPrincipalExtension
    {
		/// <summary>
		/// Method is deprecated. Use IBaseServiceContextProvider insted
		/// </summary>
		/// <param name="claimsPrincipal"></param>
		/// <param name="workspaceArtifactId"></param>
		/// <returns></returns>
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

        public static string GetSchemalessResourceDataBasePrepend(this ClaimsPrincipal claimsPrincipal, int workspaceArtifactId)
        {
            string prepend = String.Empty;
            BaseServiceContext context = GetUnversionContext(claimsPrincipal, workspaceArtifactId);
            try
            {
                prepend = context.ChicagoContext.ThreadSafeChicagoContext.DBContext.GetSchemalessResourceDataBasePrepend();
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to determine scratch table's perpend. The integration Point may be out of date.", exception);
            }
            return prepend;
        }

        public static string ResourceDBPrepend(this ClaimsPrincipal claimsPrincipal, int workspaceArtifactId)
        {
            string prepend = String.Empty;
            BaseServiceContext context = GetUnversionContext(claimsPrincipal, workspaceArtifactId);
            try
            {
                prepend = context.ChicagoContext.ThreadSafeChicagoContext.DBContext.ResourceDBPrepend();
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to determine scratch table's prepend. The integration Point may be out of date.", exception);
            }
            return prepend;
        }
    }
}