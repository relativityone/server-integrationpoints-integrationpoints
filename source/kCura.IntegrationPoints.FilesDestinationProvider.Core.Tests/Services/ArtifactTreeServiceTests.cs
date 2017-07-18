using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
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
		private IRSAPIClient _client;
		private IHelper _helper;

		private IArtifactService _artifactService;

		private ArtifactTreeService _artifactTreeService;

		private QueryResult _queryResult;
		private IArtifactTreeCreator _treeCreator;
		private APIOptions _apiOptions;

		[SetUp]
		public override void SetUp()
		{
			_client = Substitute.For<IRSAPIClient>();
			_helper = Substitute.For<IHelper>();

			_queryResult = new QueryResult { Success = true };
			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(_queryResult);

			_apiOptions = Substitute.For<APIOptions>();
			_client.APIOptions.Returns(_apiOptions);

			_artifactService = Substitute.For<ArtifactService>(_client, _helper);
			_treeCreator = Substitute.For<IArtifactTreeCreator>();

			_artifactTreeService = new ArtifactTreeService(_artifactService, _treeCreator);
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

			_artifactTreeService.GetArtifactTreeWithWorkspaceSet(artifactTypeName, destinationWorkspaceId);

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
				ParentArtifactID = 2, 
				Name = "ZZ"
			};
			var artifact2 = new Artifact
			{
				ArtifactID = 2,
				ParentArtifactID = 1, 
				Name = "AA"
			};

			_queryResult.QueryArtifacts.AddRange(new List<Artifact> { artifact1, artifact2 });
			_artifactTreeService.GetArtifactTreeWithWorkspaceSet("artifact_type");

			IOrderedEnumerable<Artifact> receivedArg = new List<Artifact> { artifact2, artifact1 }.OrderBy(n => n.Name);
			_treeCreator.Received().Create(Arg.Is<IOrderedEnumerable<Artifact>>(x => x.SequenceEqual(receivedArg)));
		}
	}
}