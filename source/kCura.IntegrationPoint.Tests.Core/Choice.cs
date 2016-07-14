using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Choice
	{
		public static Relativity.Client.DTOs.Choice CreateChoice(int workspaceArtifactId, int fieldArtifactId, string choiceName, int choiceOrder)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				int? choiceTypeId = Fields.ReadField(workspaceArtifactId, fieldArtifactId).ChoiceTypeID;

				Relativity.Client.DTOs.Choice choiceToCreate = new Relativity.Client.DTOs.Choice
				{
					ChoiceTypeID = choiceTypeId.Value,
					Name = choiceName,
					Order = choiceOrder,
					HighlightStyleID = (int)HighlightColor.Green
				};

				WriteResultSet<Relativity.Client.DTOs.Choice> writeResult;
				try
				{
					writeResult = proxy.Repositories.Choice.Create(choiceToCreate);
				}
				catch (Exception e)
				{
					throw new Exception("Error while creating choice: " + e.Message);
				}

				if (!writeResult.Success)
				{
					throw new Exception("Error while creating choice, result set failure: " + writeResult.Message);
				}

				Result<Relativity.Client.DTOs.Choice> choice = writeResult.Results.FirstOrDefault();
				Relativity.Client.DTOs.Choice choiceArtifact = choice?.Artifact;
				return choiceArtifact;
			}
		}
	}
}
