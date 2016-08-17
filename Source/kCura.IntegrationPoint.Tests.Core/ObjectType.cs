using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ObjectType
	{
		public static int CreateObjectType(int workspaceArtifactId, string objectName)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				Relativity.Client.DTOs.ObjectType objectTypeDto = new Relativity.Client.DTOs.ObjectType
				{
					Name = objectName,
					ParentArtifactTypeID = (int)ArtifactType.Case,
					SnapshotAuditingEnabledOnDelete = true,
					Pivot = true,
					CopyInstancesOnWorkspaceCreation = false,
					Sampling = true,
					PersistentLists = false,
					CopyInstancesOnParentCopy = false
				};

				WriteResultSet<Relativity.Client.DTOs.ObjectType> writeResult;
				try
				{
					writeResult = proxy.Repositories.ObjectType.Create(objectTypeDto);
				}
				catch (Exception e)
				{
					throw new Exception("Error while creating new object type: " + e.Message);
				}

				if (!writeResult.Success)
				{
					throw new Exception("Error while creating object type, result set failure: " + writeResult.Message);
				}

				Result<Relativity.Client.DTOs.ObjectType> objectType = writeResult.Results.FirstOrDefault();
				int objectTypeArtifactId = objectType.Artifact.ArtifactID;
				return objectTypeArtifactId;
			}
		}

		public static Relativity.Client.DTOs.ObjectType ReadObjectType(int workspaceArtifactId, int objectTypeArtifactId)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				ResultSet<Relativity.Client.DTOs.ObjectType> results;
				try
				{
					results = proxy.Repositories.ObjectType.Read(new Relativity.Client.DTOs.ObjectType(objectTypeArtifactId)
					{
						Fields = FieldValue.AllFields
					});
				}
				catch (Exception e)
				{
					throw new Exception("Error while reading object type: " + e.Message);
				}

				if (!results.Success)
				{
					throw new Exception("Result set failure during object read: " + results.Message);
				}

				Result<Relativity.Client.DTOs.ObjectType> objectType = results.Results.FirstOrDefault();
				Relativity.Client.DTOs.ObjectType objectTypeArtifact = objectType?.Artifact;
				return objectTypeArtifact;
			}
		}
	}
}