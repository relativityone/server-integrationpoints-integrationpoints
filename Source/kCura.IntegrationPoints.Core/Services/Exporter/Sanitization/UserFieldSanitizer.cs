using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal sealed class UserFieldSanitizer : IExportFieldSanitizer
    {
        private readonly IHelper _helper;
        private readonly ISanitizationDeserializer _sanitizationDeserializer;

        public FieldTypeHelper.FieldType SupportedType { get; } = FieldTypeHelper.FieldType.User;

        public UserFieldSanitizer(IHelper helper, ISanitizationDeserializer sanitizationDeserializer)
        {
            _helper = helper;
            _sanitizationDeserializer = sanitizationDeserializer;
        }

        public async Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue is null)
            {
                return initialValue;
            }

            int userArtifactId = GetUserArtifactId(initialValue);

            using (IUserInfoManager userInfoManager = _helper.GetServicesManager().CreateProxy<IUserInfoManager>(ExecutionIdentity.System))
            {
                string instanceUserEmail = await GetUserEmailAsync(userInfoManager, -1, userArtifactId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(instanceUserEmail))
                {
                    return instanceUserEmail;
                }

                string workspaceUserEmail = await GetUserEmailAsync(userInfoManager, workspaceArtifactID, userArtifactId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(workspaceUserEmail))
                {
                    return workspaceUserEmail;
                }
            }

            throw new InvalidExportFieldValueException($"Could not retrieve info for user with ArtifactID {userArtifactId}. " +
                                                       $"If this workspace was restored using ARM, verify if user has been properly mapped during workspace restore.");
        }

        private async Task<string> GetUserEmailAsync(IUserInfoManager userInfoManager, int workspaceArtifactId, int userArtifactId)
        {
            QueryRequest userQuery = new QueryRequest
            {
                Condition = $@"('ArtifactID' == {userArtifactId})"
            };

            UserInfoQueryResultSet instanceUserQueryResult = await userInfoManager.RetrieveUsersBy(workspaceArtifactId, userQuery, 0, 1).ConfigureAwait(false);
            return instanceUserQueryResult?.DataResults?.SingleOrDefault()?.Email;
        }

        private int GetUserArtifactId(object initialValue)
        {
            UserInfo userFieldValue = _sanitizationDeserializer.DeserializeAndValidateExportFieldValue<UserInfo>(initialValue.ToString());
            return userFieldValue.ArtifactID;
        }
    }
}
