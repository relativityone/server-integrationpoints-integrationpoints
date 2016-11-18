using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
	[TestFixture]
	public class ArtifactTreeServiceTests : TestBase
	{
		private ArtifactTreeService _artifactTreeService;
		private IRSAPIClient _client;
		private QueryResult _queryResult;
		private IArtifactTreeCreator _treeCreator;
		private APIOptions _apiOptions;

		[SetUp]
		public override void SetUp()
		{
			_client = Substitute.For<IRSAPIClient>();
			_treeCreator = Substitute.For<IArtifactTreeCreator>();
			var helper = Substitute.For<IHelper>();
			_apiOptions = Substitute.For<APIOptions>();

			_queryResult = new QueryResult {Success = true};
			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(_queryResult);
			_client.APIOptions.Returns(_apiOptions);

			_artifactTreeService = new ArtifactTreeService(_client, _treeCreator, helper);
		}

		[Test]
		public void ItShouldQueryForGivenArtifactType()
		{
			const string artifactTypeName = "artifact_type";

			_artifactTreeService.GetArtifactTreeWithWorkspaceSet(artifactTypeName);

			_client.Received().Query(Arg.Any<APIOptions>(), Arg.Is<Query>(x => x.ArtifactTypeName.Equals(artifactTypeName)));
			
		}

		[Test]
		public void ItShouldQueryForGivenArtifactTypeAndWorkspaceId()
		{
			const string artifactTypeName = "artifact_type";
			const int destinationWorkspaceId = 1024178;

			_artifactTreeService.GetArtifactTreeWithWorkspaceSet(artifactTypeName,destinationWorkspaceId);

			_client.Received().Query(Arg.Any<APIOptions>(), Arg.Is<Query>(x => x.ArtifactTypeName.Equals(artifactTypeName)));

			Assert.That(_apiOptions.WorkspaceID, Is.EqualTo(destinationWorkspaceId));
		}

		[Test]
		public void ItShouldThrowExceptionWhenQueryFailed()
		{
			_queryResult.Success = false;

			Assert.That(() => _artifactTreeService.GetArtifactTreeWithWorkspaceSet("type"),
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

			_artifactTreeService.GetArtifactTreeWithWorkspaceSet("artifact_type");

			_treeCreator.Received().Create(Arg.Is<IList<Artifact>>(x => x.SequenceEqual(new List<Artifact> {artifact2, artifact1})));
		}
	}
}