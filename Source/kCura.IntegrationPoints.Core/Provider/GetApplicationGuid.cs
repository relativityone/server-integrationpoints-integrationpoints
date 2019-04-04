using LanguageExt;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Core.Provider
{
    public class GetApplicationGuid
    {
        private readonly IDBContext _caseDBcontext;

        public GetApplicationGuid(IDBContext caseDBcontext)
        {
            _caseDBcontext = caseDBcontext;
        }

        public Either<string, Guid> Execute(int applicationID)
        {
            return GetApplicationGuidObjectFromDatabase(applicationID)
                .Bind(ConvertGuidObjectValueToGuid);
        }

        private Either<string, object> GetApplicationGuidObjectFromDatabase(int applicationID)
        {
            string sql = Data.Resources.Resource.GetApplicationGuid;
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@ApplicationID", applicationID)
            };

            try
            {
                object applicationGuidValueAsObject = _caseDBcontext.ExecuteSqlStatementAsScalar(sql, sqlParams.ToArray());

                if (applicationGuidValueAsObject == null)
                {
                    return $"Application Guid is not available for given ApplicationID: {applicationID}";
                }

                return applicationGuidValueAsObject;
            }
            catch (Exception ex)
            {
                return $"Exception occured while querying for Application Guid. ApplicationID: {applicationID}, Exception: {ex}";
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
