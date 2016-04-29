﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiTabRepository : ITabRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public RsapiTabRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public int? RetrieveTabArtifactId(int objectTypeArtifactId, string tabName)
		{
			var tabNameCondition = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, tabName);
			var objectTypeCondition = new WholeNumberCondition(FieldFieldNames.ObjectType, NumericConditionEnum.EqualTo, objectTypeArtifactId);
			var compositeCondition = new CompositeCondition(tabNameCondition, CompositeConditionEnum.And, objectTypeCondition);

			var tabQuery = new Query<Tab>()
			{
				Fields = FieldValue.AllFields,
				Condition = compositeCondition
			};


			QueryResultSet<Tab> resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				resultSet = rsapiClient.Repositories.Tab.Query(tabQuery);
			}


			if (!resultSet.Success)
			{
				throw new Exception($"Unable to retrieve the '{tabName}' tab: {resultSet.Message}");
			}

			Result<Tab> tab = resultSet.Results.FirstOrDefault();

			return tab?.Artifact.ArtifactID;
		}

		public int? RetrieveTabArtifactIdByGuid(string tabGuid)
		{
			var tabQuery = new Query<Tab>();

			QueryResultSet<Tab> resultSet;
			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

					resultSet = rsapiClient.Repositories.Tab.Query(tabQuery);
				}
			}
			catch (Exception ex)
			{
				
				throw new Exception("Unable to retrieve tab.", ex);
			}

			if (!resultSet.Success)
			{
				throw new Exception($"Unable to retrieve tab: {resultSet.Message}");
			}
		
			int? tabArtifactId = null;
			Guid tabGuidToFind = new Guid(tabGuid);
			foreach (Result<Tab> tab in resultSet.Results)
			{
				if (tab.Artifact.Guids.Contains(tabGuidToFind))
				{
					tabArtifactId = tab.Artifact.ArtifactID;
					break;
				}
			}

			return tabArtifactId;
		}

		public void Delete(int artifactId)
		{
			var artifactRequest = new List<ArtifactRequest>()
			{
				new ArtifactRequest((int)ArtifactType.Tab, artifactId)
			};

			ResultSet resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				resultSet = rsapiClient.Delete(rsapiClient.APIOptions, artifactRequest);
			}

			if (!resultSet.Success)
			{
				throw new Exception($"Unable to delete tab: {resultSet.Message}");
			}
		}
	}
}