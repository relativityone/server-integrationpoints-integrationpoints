using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.DbContext;
using LanguageExt;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public class ApplicationGuidFinder : IApplicationGuidFinder
    {
        private readonly IWorkspaceDBContext _caseDBcontext;

        public ApplicationGuidFinder(IWorkspaceDBContext caseDBcontext)
        {
            _caseDBcontext = caseDBcontext;
        }

        public Either<string, Guid> GetApplicationGuid(int workspaceApplicationID)
        {
            return GetApplicationGuidObjectFromDatabase(workspaceApplicationID)
                .Bind(ConvertGuidObjectValueToGuid);
        }

        private Either<string, object> GetApplicationGuidObjectFromDatabase(int workspaceApplicationID)
        {
            string sql = Data.Resources.Resource.GetApplicationGuid;
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@ApplicationID", workspaceApplicationID)
            };

            try
            {
                object applicationGuidValueAsObject = _caseDBcontext.ExecuteSqlStatementAsScalar(sql, sqlParams.ToArray());

                if (applicationGuidValueAsObject == null)
                {
                    return $"Application Guid is not available for given ApplicationID: {workspaceApplicationID}";
                }

                return applicationGuidValueAsObject;
            }
            catch (Exception ex)
            {
                return $"Exception occured while querying for Application Guid. ApplicationID: {workspaceApplicationID}, Exception: {ex}";
            }
        }

        private static Either<string, Guid> ConvertGuidObjectValueToGuid(object applicationGuidValueAsObject)
        {
            string applicationGuidValueAsString = applicationGuidValueAsObject.ToString();

            Guid result;
            if (Guid.TryParse(applicationGuidValueAsString, out result))
            {
                return result;
            }

            return $"Application Guid value is in invalid format. Value: {applicationGuidValueAsString}";
        }
    }
}
