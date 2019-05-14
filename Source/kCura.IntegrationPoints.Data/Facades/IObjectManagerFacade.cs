﻿using System;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;

namespace kCura.IntegrationPoints.Data.Facades
{
	internal interface IObjectManagerFacade : IDisposable
	{
		Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest);

		Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request);

		Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request);

		Task<MassUpdateResult> UpdateAsync(
			int workspaceArtifactID,
			MassUpdateByObjectIdentifiersRequest request,
			MassUpdateOptions updateOptions);

		Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request);

		Task<QueryResult> QueryAsync(
			int workspaceArtifactID,
			QueryRequest request,
			int start,
			int length);

		Task<IKeplerStream> StreamLongTextAsync(
			int workspaceArtifactID,
			RelativityObjectRef exportObject,
			FieldRef longTextField);
	}
}
