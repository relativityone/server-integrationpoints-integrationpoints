using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.DbContext;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Toggles;
using Relativity.Sync.Utils;
using Relativity.Toggles;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Returns an user's email given its exported representation.
    /// Import API expects the email address instead of the ArtifactID.
    /// </summary>
    internal sealed class UserFieldSanitizer : IExportFieldSanitizer
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IMemoryCache _memoryCache;
        private readonly IEddsDbContext _eddsDbContext;
        private readonly IAPILog _log;
        private readonly IToggleProvider _toggleProvider;
        private readonly JSONSerializer _serializer = new JSONSerializer();
        private readonly CacheItemPolicy _memoryCacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) };

        public RelativityDataType SupportedType { get; } = RelativityDataType.User;

        public UserFieldSanitizer(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IMemoryCache memoryCache,
            IEddsDbContext eddsDbContext, IAPILog log, IToggleProvider toggleProvider)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _memoryCache = memoryCache;
            _eddsDbContext = eddsDbContext;
            _log = log;
            _toggleProvider = toggleProvider;
        }

        public async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue is null)
            {
                return initialValue;
            }

            int userArtifactId = GetUserArtifactIdFromInitialValue(initialValue);

            string cacheKey = $"{nameof(UserFieldSanitizer)}_{userArtifactId}";

            string cacheUserEmail = _memoryCache.Get<string>(cacheKey);
            if (!String.IsNullOrEmpty(cacheUserEmail))
            {
                return cacheUserEmail;
            }

            int instanceUserArtifactId;
            if (await _toggleProvider.IsEnabledAsync<DisableUserMapWithSQLToggle>().ConfigureAwait(false))
            {
                _log.LogInformation("DisableUserMapWithSQL is enabled - rewrite UserArtifactId to InstanceArtifactId: {userArtifactId}", userArtifactId);
                instanceUserArtifactId = userArtifactId;
            }
            else
            {
                _log.LogInformation("Read Instance Level UserId from UserCaseUser for UserArtifactId {userArtifactId}", userArtifactId);
                instanceUserArtifactId = GetInstanceUserArtifactId(userArtifactId, workspaceArtifactId);
                _log.LogInformation("Read Instance Level UserId: {instanceUserArtifactId}", instanceUserArtifactId);
            }

            using (IUserInfoManager userInfoManager = await _serviceFactoryForAdmin.CreateProxyAsync<IUserInfoManager>().ConfigureAwait(false))
            {
                string instanceUserEmail = await GetUserEmailAsync(userInfoManager, -1, instanceUserArtifactId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(instanceUserEmail))
                {
                    _memoryCache.Add(cacheKey, instanceUserEmail, _memoryCacheItemPolicy);
                    return instanceUserEmail;
                }

                string workspaceUserEmail = await GetUserEmailAsync(userInfoManager, workspaceArtifactId, instanceUserArtifactId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(workspaceUserEmail))
                {
                    _memoryCache.Add(cacheKey, workspaceUserEmail, _memoryCacheItemPolicy);
                    return workspaceUserEmail;
                }
            }

            throw new SyncItemLevelErrorException($"Could not retrieve info for user with ArtifactID {userArtifactId}. " +
                "If this workspace was restored using ARM, verify if user has been properly mapped during workspace restore.");
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

        private int GetUserArtifactIdFromInitialValue(object initialValue)
        {
            UserInfo userFieldValue;
            try
            {
                userFieldValue = _serializer.Deserialize<UserInfo>(initialValue.ToString());
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                throw new InvalidExportFieldValueException($"Expected value to be deserializable to {typeof(UserInfo)}, but instead type was {initialValue.GetType()}.", ex);
            }

            return userFieldValue.ArtifactID;
        }

        private int GetInstanceUserArtifactId(int userArtifactId, int workspaceArtifactId)
        {
            try
            {
                string sqlStatement =
                    $"SELECT [UserArtifactId] FROM [eddsdbo].[UserCaseUser] WHERE [CaseUserArtifactID] = {userArtifactId} AND [CaseArtifactID] = {workspaceArtifactId}";
                int instanceUserArtifactId = _eddsDbContext.ExecuteSqlStatementAsScalar<int>(sqlStatement);

                if (instanceUserArtifactId == 0)
                {
                    _log.LogWarning("Invalid InstanceUserArtifactID: {instanceUserArtifactId} was returned for UserArtifactId {userArtifactId}",
                        instanceUserArtifactId, userArtifactId);
                    return userArtifactId;
                }

                return instanceUserArtifactId;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Expection was thrown when reading Instance User Artifact ID from UserCaseUser for User: {userArtifactId} and Workspace: {workspaceArtifactId}",
                    userArtifactId, workspaceArtifactId);
            }

            return userArtifactId;
        }
    }
}
