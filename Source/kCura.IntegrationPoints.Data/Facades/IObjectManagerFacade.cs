using System;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Facades
{
	internal interface IObjectManagerFacade : IDisposable
	{
		Task<CreateResult> CreateAsync(int workspaceArtifactId, CreateRequest createRequest);
		Task<ReadResult> ReadAsync(int workspaceArtifactId, ReadRequest request);
		Task<UpdateResult> UpdateAsync(int workspaceArtifactId, UpdateRequest request);
		Task<DeleteResult> DeleteAsync(int workspaceArtifactId, DeleteRequest request);
		Task<QueryResult> QueryAsync(int workspaceArtifactId, QueryRequest request, int start, int length);
	}
}
