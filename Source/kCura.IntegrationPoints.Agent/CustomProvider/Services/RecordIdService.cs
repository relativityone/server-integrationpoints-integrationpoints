using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    public class RecordIdService : IRecordIdService
    {
        public RecordIdService()
        {
        }

        public async Task<List<CustomProviderBatch>> BuildIdFilesAsync(IDataSourceProvider provider, IntegrationPointDto integrationPoint, string directoryPath)
        {
            const int batchCapacity = 20;
            IDataReader reader = await GetIdsReaderAsync(provider, integrationPoint).ConfigureAwait(false);

            List<CustomProviderBatch> batches = new List<CustomProviderBatch>();
            List<string> batchIDs = new List<string>(batchCapacity);
            int batchIndex = 1;

            while (reader.Read())
            {
                string recordId = reader.GetString(0);
                batchIDs.Add(recordId);

                if (batchIDs.Count == batchCapacity)
                {
                    string batchIDsFileName = $"{batchIndex.ToString().PadLeft(7, '0')}.id";
                    string batchIDsFilePath = Path.Combine(directoryPath, batchIDsFileName);
                    File.WriteAllLines(batchIDsFilePath, batchIDs);

                    batches.Add(new CustomProviderBatch()
                    {
                        BatchID = batchIndex,
                        IDsFilePath = batchIDsFilePath
                    });

                    batchIndex++;
                    batchIDs.Clear();
                }
            }

            return batches;
        }

        private Task<IDataReader> GetIdsReaderAsync(IDataSourceProvider provider, IntegrationPointDto integrationPoint)
        {
            FieldEntry idField = integrationPoint.FieldMappings.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier)?.SourceField;
            IDataReader reader = provider.GetBatchableIds(idField, new DataSourceProviderConfiguration(integrationPoint.SourceConfiguration, integrationPoint.SecuredConfiguration));
            return Task.FromResult(reader);
        }
    }
}
