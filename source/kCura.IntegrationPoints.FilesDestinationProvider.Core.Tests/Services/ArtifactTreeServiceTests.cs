using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
	public class ArtifactTreeServiceTests
	{
		private ArtifactTreeService _artifactTreeService;
		private IRSAPIClient _client;
		private QueryResult _queryResult;
		private IArtifactTreeCreator _treeCreator;

		[SetUp]
		public void SetUp()
		{
			_client = Substitute.For<IRSAPIClient>();
			_treeCreator = Substitute.For<IArtifactTreeCreator>();
			var helper = Substitute.For<IHelper>();

			_queryResult = new QueryResult {Success = true};
			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(_queryResult);

			_artifactTreeService = new ArtifactTreeService(_client, _treeCreator, helper);
		}

		[Test]
		public void ItShouldQueryForGivenArtifactType()
		{
			const string artifactTypeName = "artifact_type";

			_artifactTreeService.GetArtifactTree(artifactTypeName);

			_client.Received().Query(Arg.Any<APIOptions>(), Arg.Is<Query>(x => x.ArtifactTypeName.Equals(artifactTypeName)));
		}

		[Test]
		public void ItShouldThrowExceptionWhenQueryFailed()
		{
			_queryResult.Success = false;

			Assert.That(() => _artifactTreeService.GetArtifactTree("type"),
				Throws.Exception
					.TypeOf<NotFoundException>());
		}

		[Test]
		public void ItShouldCreateTreeUsingQueryResult()
		{
			var artifact1 = new Artifact
			{
				ArtifactID = 1,
				ParentArtifactID = 2
			};
			var artifact2 = new Artifact
			{
				ArtifactID = 2,
				ParentArtifactID = 1
			};

			_queryResult.QueryArtifacts.AddRange(new List<Artifact> {artifact2, artifact1});

			_artifactTreeService.GetArtifactTree("artifact_type");

			_treeCreator.Received().Create(Arg.Is<IList<Artifact>>(x => x.SequenceEqual(new List<Artifact> {artifact2, artifact1})));
		}
	}
}