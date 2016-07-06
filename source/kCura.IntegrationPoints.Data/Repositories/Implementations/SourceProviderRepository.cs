﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceProviderRepository : ISourceProviderRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public SourceProviderRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public SourceProviderDTO Read(int artifactId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.SourceProvider),
				Condition = new WholeNumberCondition(new Guid(Domain.Constants.SOURCEPROVIDER_ARTIFACTID_FIELD), NumericConditionEnum.EqualTo, artifactId),
				Fields = new List<FieldValue>()
				{
					new FieldValue(new Guid(SourceProviderFieldGuids.Name)),
					new FieldValue(new Guid(SourceProviderFieldGuids.Identifier))
				}
			};

			QueryResultSet<RDO> queryResultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					queryResultSet = rsapiClient.Repositories.RDO.Query(query, 1);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to retrieve Source Provider: {e.Message}", e);
				}
			}

			Result<RDO> result = queryResultSet.Results.FirstOrDefault();
			if (!queryResultSet.Success || result == null)
			{
				throw new Exception($"Unable to retrieve Source Provider: {queryResultSet.Message}");
			}

			var dto = new SourceProviderDTO()
			{
				ArtifactId = result.Artifact.ArtifactID,
				Name = result.Artifact.Fields[0].ValueAsFixedLengthText,
				Identifier = new Guid(result.Artifact.Fields[1].ValueAsFixedLengthText)
			};

			return dto;
		}

		public int GetArtifactIdFromSourceProviderTypeGuidIdentifier(string sourceProviderGuidIdentifier)
		{
			int sourceProviderArtifactId;

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.SourceProvider),
				Condition = new TextCondition(new Guid(SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, sourceProviderGuidIdentifier),
				Fields = new List<FieldValue>()
				{
					new FieldValue("Artifact ID")
				}
			};

			QueryResultSet<RDO> results = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				results = rsapiClient.Repositories.RDO.Query(query, 1);
			}

			if (!results.Success)
			{
				throw new Exception($"Unable to retrieve Source Provider: {results.Message}");
			}

			sourceProviderArtifactId = results.Results.Select(result => result.Artifact.ArtifactID).FirstOrDefault();

			if (sourceProviderArtifactId == 0)
			{
				throw new Exception($"Unable to retrieve Source Provider ({sourceProviderGuidIdentifier}).");
			}

			return sourceProviderArtifactId;
		}
	}
}