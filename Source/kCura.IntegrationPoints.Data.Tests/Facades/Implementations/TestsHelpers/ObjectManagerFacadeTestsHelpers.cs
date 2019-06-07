﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Facades;
using Moq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations.TestsHelpers
{
	internal static class ObjectManagerFacadeTestsHelpers
	{
		public static Expression<Func<IObjectManagerFacade, Task<CreateResult>>> CreateCallWithAnyArgs => x => x.CreateAsync(
			It.IsAny<int>(),
			It.IsAny<CreateRequest>()
		);

		public static Expression<Func<IObjectManagerFacade, Task<ReadResult>>> ReadCallWithAnyArgs => x => x.ReadAsync(
			It.IsAny<int>(),
			It.IsAny<ReadRequest>()
		);

		public static Expression<Func<IObjectManagerFacade, Task<UpdateResult>>> UpdateCallWithAnyArgs => x => x.UpdateAsync(
			It.IsAny<int>(),
			It.IsAny<UpdateRequest>()
		);

		public static Expression<Func<IObjectManagerFacade, Task<MassUpdateResult>>> MassUpdateCallWithAnyArgs => x => x.UpdateAsync(
			It.IsAny<int>(),
			It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
			It.IsAny<MassUpdateOptions>()
		);

		public static Expression<Func<IObjectManagerFacade, Task<DeleteResult>>> DeleteCallWithAnyArgs => x => x.DeleteAsync(
			It.IsAny<int>(),
			It.IsAny<DeleteRequest>()
		);

		public static Expression<Func<IObjectManagerFacade, Task<QueryResult>>> QueryCallWithAnyArgs => x => x.QueryAsync(
			It.IsAny<int>(),
			It.IsAny<QueryRequest>(),
			It.IsAny<int>(),
			It.IsAny<int>()
		);
	}
}
