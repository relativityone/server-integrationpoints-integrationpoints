using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IRelativityObjectManager
	{
		int Create<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		int Create(ObjectTypeRef objectType, List<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		int Create(ObjectTypeRef objectType, RelativityObjectRef parentObject, List<FieldRefValuePair> fieldValues,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		T Read<T>(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		T Read<T>(int artifactId, IEnumerable<Guid> fieldsGuids, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();

		bool Update(int artifactId, IList<FieldRefValuePair> fieldsValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		bool Update<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();

		Task<bool> MassUpdateAsync(
			IEnumerable<int> objectsIDs,
			IEnumerable<FieldRefValuePair> fieldsToUpdate,
			FieldUpdateBehavior fieldUpdateBehavior,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		bool Delete<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		bool Delete(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		Task<bool> MassDeleteAsync(
			IEnumerable<int> objectsIDs,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		ResultSet<T> Query<T>(QueryRequest q, int start, int length, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		List<T> Query<T>(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		Task<List<T>> QueryAsync<T>(QueryRequest q, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		List<RelativityObject> Query(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<List<RelativityObject>> QueryAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		ResultSet<RelativityObject> Query(QueryRequest q, int start, int length, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<ResultSet<RelativityObject>> QueryAsync(QueryRequest q, int start, int length, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		int QueryTotalCount(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<int> QueryTotalCountAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		System.IO.Stream StreamUnicodeLongText(int relativityObjectArtifactId, FieldRef longTextFieldRef, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		System.IO.Stream StreamNonUnicodeLongText(int relativityObjectArtifactId, FieldRef longTextFieldRef, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		Task<ExportInitializationResults> InitializeExportAsync(QueryRequest queryRequest, int start, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(Guid runID, int resultsBlockSize, int exportIndexID, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		Task<IExportQueryResult> QueryWithExportAsync(QueryRequest queryRequest, int start,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		int GetWorkspaceID_Deprecated();
	}
}
