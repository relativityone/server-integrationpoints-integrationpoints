using Moq;
using Relativity.Services.ChoiceQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ChoiceQueryManagerStub: KeplerStubBase<IChoiceQueryManager>
    {
		public void SetupArtifactGuidManager()
		{
			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<int>(a => a == 1234567)))
				.Returns((int workspaceId, int fieldArtifactId) =>
				{
					return Task.FromResult(
							new List<global::Relativity.Services.ChoiceQuery.Choice>()
							{
								new global::Relativity.Services.ChoiceQuery.Choice()
								{
									ArtifactID = 1039894,
									Name = "Append Only"
								},
								new global::Relativity.Services.ChoiceQuery.Choice()
								{
									ArtifactID = 1039895,
									Name = "Append/Overlay"
								},
								new global::Relativity.Services.ChoiceQuery.Choice()
								{
									ArtifactID = 1039896,
									Name = "Overlay Only"
								}
							}
						);
				});
		}
	}
}
