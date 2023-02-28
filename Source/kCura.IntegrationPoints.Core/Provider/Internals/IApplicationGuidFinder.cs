using System;
using LanguageExt;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public interface IApplicationGuidFinder
    {
        /// <summary>
        /// Returns application GUID
        /// </summary>
        /// <param name="workspaceApplicationID">Application ID in a workspace</param>
        /// <returns></returns>
        Either<string, Guid> GetApplicationGuid(int workspaceApplicationID);
    }
}
