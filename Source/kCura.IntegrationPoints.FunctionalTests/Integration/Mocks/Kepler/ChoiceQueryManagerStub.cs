using Moq;
using Relativity.Services.ChoiceQuery;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class ChoiceQueryManagerStub : KeplerStubBase<IChoiceQueryManager>
	{
		public void SetupArtifactGuidManager()
		{
			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int workspaceId, int fieldArtifactId) =>
				{
					List<Choice> result = new List<Choice>();

					if (IsQueryForOverwrite(fieldArtifactId))
					{
						result = Const.Choices.OverwriteFields;
					}

					return Task.FromResult(result);
				});
		}

		private bool IsQueryForOverwrite( int fieldArtifactId)
        {
			return fieldArtifactId == Const.OVERWRITE_FIELD_ARTIFACT_ID;
        }
	}
}
