using System;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Facades
{
	internal interface IObjectManagerFacade : IDisposable
	{
		Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest);
		Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request);
		Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request);
		Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request);
		Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length);
	}
}
