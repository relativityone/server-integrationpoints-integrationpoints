using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Performance.Helpers
{
    public class AzureTableHelper
    {
        private readonly CloudTableClient _tableClient;

        public AzureTableHelper(string connectionString)
        {
            CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount storageAccount);

            _tableClient = storageAccount.CreateCloudTableClient();
        }

        public static AzureTableHelper CreateFromTestConfig()
        {
            return new AzureTableHelper(AppSettings.AzureStorageConnectionString);
        }

        public Task InsertAsync(string tableName, TableEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            CloudTable table = _tableClient.GetTableReference(tableName);

            TableOperation insertOperation = TableOperation.Insert(entity);

            return table.ExecuteAsync(insertOperation);
        }

        public IEnumerable<T> QueryAll<T>(string tableName)
            where T : TableEntity, new()
        {
            CloudTable table = _tableClient.GetTableReference(tableName);

            return table.CreateQuery<T>();
        }
    }
}
