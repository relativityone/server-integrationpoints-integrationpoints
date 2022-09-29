using System;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager
{
    internal interface IObjectManagerFacade : IDisposable
    {
        Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest);

        Task<MassCreateResult> CreateAsync(int workspaceArtifactID, MassCreateRequest createRequest);

        Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request);

        Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request);

        Task<MassUpdateResult> UpdateAsync(
            int workspaceArtifactID,
            MassUpdateByObjectIdentifiersRequest request,
            MassUpdateOptions updateOptions);

        Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request);

        Task<MassDeleteResult> DeleteAsync(int workspaceArtifactID, MassDeleteByObjectIdentifiersRequest request);

        Task<QueryResult> QueryAsync(
            int workspaceArtifactID,
            QueryRequest request,
            int start,
            int length);

        Task<IKeplerStream> StreamLongTextAsync(
            int workspaceArtifactID,
            RelativityObjectRef exportObject,
            FieldRef longTextField);

        Task<ExportInitializationResults> InitializeExportAsync(
            int workspaceArtifactID,
            QueryRequest queryRequest,
            int start);

        Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
            int workspaceArtifactID, 
            Guid runID,
            int resultsBlockSize,
            int exportIndexID);
    }
}
