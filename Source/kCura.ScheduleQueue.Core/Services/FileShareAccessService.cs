using kCura.ScheduleQueue.Core.Interfaces;
using Relativity.API;
using Relativity.DataMigration.MigrateFileshareAccess;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Credential;
using Relativity.Services;
using System.Collections.Generic;
using Relativity.Services.Interfaces.ResourceServer.Models;
using Relativity.Services.Interfaces.ResourceServer;
using IResourceServerManager = Relativity.Services.ResourceServer.IResourceServerManager;
using Relativity.Services.ResourceServer;

namespace kCura.ScheduleQueue.Core.Services
{
    public class FileShareAccessService : IFileShareAccessService
    {
        private const string _SQL_PRIMARY_SERVER_TYPE = "SQL - Primary";
        private const string _SQL_DISTRIBUTED_SERVER_TYPE = "SQL - Distributed";
        private const string _REL_SERVICE_NAME_PREFIX = "relsvc-t";

        private readonly IHelper _helper;
        private readonly IAPILog _logger;
        private readonly IMigrateFileshareFactory _fileShareAccessFactory;

        public FileShareAccessService(IHelper helper, IMigrateFileshareFactory fileShareAccessFactory)
        {
            _helper = helper;
            _fileShareAccessFactory = fileShareAccessFactory;
            
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<FileShareAccessService>();
        }

        public async Task MountBcpPathAsync()
        {
            try
            {
                _logger.LogInformation("Mounting BCPPath...");

                FileShareInfo bcpFileShareInfo = await GetBcpFileShareInfoAsync().ConfigureAwait(false);

                if (bcpFileShareInfo == null)
                {
                    _logger.LogWarning("Skip mounting BCPPath Fileshare. Failed to retrieve BCPPath Fileshare information.");
                    return;
                }

                using (IMigrateFileshare migrateFileShare = _fileShareAccessFactory.Create())
                {
                    await migrateFileShare.MountAsync(
                            bcpFileShareInfo.UncPath,
                            bcpFileShareInfo.AccountName,
                            bcpFileShareInfo.AccountPassword)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during BCPPath Fileshare mount. Skipping...");
                return;
            }
        }

        private async Task<FileShareInfo> GetBcpFileShareInfoAsync()
        {
            FileShareInfo fileShareInfo = new FileShareInfo();

            using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            using (IResourceServerManager resourceServerManager = _helper.GetServicesManager().CreateProxy<IResourceServerManager>(ExecutionIdentity.System))
            {
                var queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef() { Name = "Resource Server" },
                    Condition = $"'Type' IN ['{_SQL_DISTRIBUTED_SERVER_TYPE}', '{_SQL_PRIMARY_SERVER_TYPE}'] AND 'Status' == 'Active'",
                };

                QueryResultSlim result = await objectManager.QuerySlimAsync(-1, queryRequest, 0, int.MaxValue).ConfigureAwait(false);

                if(result == null || result.ResultCount == 0)
                {
                    _logger.LogWarning("SQL Servers were not found.");
                    return null;
                }

                string resourceServerBcpPath = null;

                using(ISqlPrimaryServerManager sqlPrimaryServerManager = _helper.GetServicesManager().CreateProxy<ISqlPrimaryServerManager>(ExecutionIdentity.System))
                using(ISqlDistributedServerManager sqlDistributedServerManager = _helper.GetServicesManager().CreateProxy<ISqlDistributedServerManager>(ExecutionIdentity.System))
                {
                    foreach (var obj in result.Objects)
                    {
                        if(!string.IsNullOrEmpty(resourceServerBcpPath))
                        {
                            break;
                        }

                        ResourceServer serverResponse = await resourceServerManager.ReadSingleAsync(obj.ArtifactID).ConfigureAwait(false);
                        if (serverResponse?.ServerType?.Name == _SQL_PRIMARY_SERVER_TYPE)
                        {
                            SqlPrimaryServerResponse sqlPrimaryResponse = await sqlPrimaryServerManager.ReadAsync(serverResponse.ArtifactID).ConfigureAwait(false);
                            resourceServerBcpPath = sqlPrimaryResponse?.BcpPath;
                        }
                        else if(serverResponse?.ServerType?.Name == _SQL_DISTRIBUTED_SERVER_TYPE)
                        {
                            SqlDistributedServerResponse sqlPrimaryResponse = await sqlDistributedServerManager.ReadAsync(serverResponse.ArtifactID).ConfigureAwait(false);
                            resourceServerBcpPath = sqlPrimaryResponse?.BcpPath;
                        }
                    }
                }

                if(string.IsNullOrEmpty(resourceServerBcpPath))
                {
                    _logger.LogWarning("BCPPath is not set for any of the SQL Servers");
                    return null;
                }

                fileShareInfo.UncPath = resourceServerBcpPath;
            }

            using (ICredentialManager credentialManager = _helper.GetServicesManager().CreateProxy<ICredentialManager>(ExecutionIdentity.System))
            {
                var query = new Query { Condition = "'CredentialType' LIKE 'FileAccess'" };

                CredentialQueryResultSet result = await credentialManager.QueryAsync(query).ConfigureAwait(false);

                if(result == null)
                {
                    _logger.LogWarning("Credential for Type 'FileAccess' was not found. Result is null");
                    return null;
                }

                if(!result.Success)
                {
                    _logger.LogWarning("Credential for Type 'FileAccess' was not found. Query finished up with result.Success {credQueryStatus}", result.Success);
                    return null;
                }

                if(result.Results.Count == 0)
                {
                    _logger.LogWarning("Credential for Type 'FileAccess' was not found. Query finished up with empty results");
                    return null;
                }

                FileShareCredential credential = result.Results.Select(x => new FileShareCredential(x.Artifact?.SecretValues))
                    .FirstOrDefault(fs =>
                        !string.IsNullOrWhiteSpace(fs.AccountName) &&
                        fs.AccountName.StartsWith(_REL_SERVICE_NAME_PREFIX, StringComparison.OrdinalIgnoreCase));

                if(credential == null)
                {
                    _logger.LogWarning("Credential applicable to mount BCPPath not found");
                    return null;
                }

                fileShareInfo.AccountName = credential.AccountName;
                fileShareInfo.AccountPassword = credential.AccountPassword;

                if(string.IsNullOrEmpty(fileShareInfo.AccountName))
                {
                    _logger.LogWarning("AccountName is empty.");
                    return null;
                }

                if (string.IsNullOrEmpty(fileShareInfo.AccountPassword))
                {
                    _logger.LogWarning("AccountPassword is empty.");
                    return null;
                }

                return fileShareInfo;
            }
        }

        private class FileShareInfo
        {
            public string UncPath { get; set; }

            public string AccountName { get; set; }

            public string AccountPassword { get; set; }
        }

        private class FileShareCredential
        {
            public const string AccountNameKey = "accountName";
            public const string AccountPasswordKey = "accountPassword";

            private readonly Dictionary<string, string> secretValues;

            public string AccountName => GetSecretValue(AccountNameKey);

            public string AccountPassword => GetSecretValue(AccountPasswordKey);

            public FileShareCredential(Dictionary<string, string> secretValues)
            {
                this.secretValues = secretValues;
            }

            private string GetSecretValue(string secretKey)
            {
                if (secretValues == null || secretValues.Count == 0)
                {
                    return null;
                }

                return secretValues.ContainsKey(secretKey) ? secretValues[secretKey] : null;
            }
        }
    }
}
