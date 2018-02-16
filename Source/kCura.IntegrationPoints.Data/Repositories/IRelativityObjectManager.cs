﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IRelativityObjectManager
	{
		int Create<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();

		T Read<T>(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		T Read<T>(int artifactId, IEnumerable<Guid> fieldsGuids, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();

		bool Update<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();

		bool Delete<T>(T relativityObject, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		bool Delete(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		ResultSet<T> Query<T>(QueryRequest q, int start, int length, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		List<T> Query<T>(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		Task<List<T>> QueryAsync<T>(QueryRequest q, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new();
		List<RelativityObject> Query(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<List<RelativityObject>> QueryAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		int QueryTotalCount(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		Task<int> QueryTotalCountAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
	}
}