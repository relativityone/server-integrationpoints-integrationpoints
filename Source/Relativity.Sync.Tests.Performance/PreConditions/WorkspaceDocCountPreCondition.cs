﻿using System;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Extensions;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
	internal class WorkspaceDocCountPreCondition : IPreCondition
	{
		private readonly ServiceFactory _serviceFactory;
		private readonly int _workspaceId;
		private readonly int _expectedDocCount;

		public string Name => $"{nameof(WorkspaceDocCountPreCondition)} - {_workspaceId}";

		public WorkspaceDocCountPreCondition(ServiceFactory serviceFactory, int workspaceId, int expectedDocCount)
		{
			_serviceFactory = serviceFactory;
			_workspaceId = workspaceId;
			_expectedDocCount = expectedDocCount;
		}

		public bool Check()
		{
			using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document }
				};

				var result = objectManager.QueryAllAsync(_workspaceId, request)
					.GetAwaiter().GetResult();

				return result.Count == _expectedDocCount;
			}
		}

		public FixResult TryFix()
		{
			return FixResult.Error(new Exception(
				$"{nameof(WorkspaceDocCountPreCondition)} - Document Count check can't be fixed. Workspace is corrupted"));
		}
	}
}
