using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture]
	public class ArtifactTreeCreatorTests : TestBase
	{
		private ArtifactTreeCreator _treeByParentIdCreator;

		[SetUp]
		public override void SetUp()
		{
			var helper = Substitute.For<IHelper>();
			_treeByParentIdCreator = new ArtifactTreeCreator(helper);
		}

		[Test]
		public void ItShouldThrowExceptionWhenRootNotFound()
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

			Assert.That(() => _treeByParentIdCreator.Create(new List<Artifact> {artifact1, artifact2}),
				Throws.Exception
					.TypeOf<NotFoundException>());
		}

		[Test]
		public void ItShouldThrowExceptionWhenMoreThanOneRootFound()
		{
			var artifact1 = new Artifact
			{
				ArtifactID = 1
			};
			var artifact2 = new Artifact
			{
				ArtifactID = 2
			};

			Assert.That(() => _treeByParentIdCreator.Create(new List<Artifact> {artifact1, artifact2}),
				Throws.Exception
					.TypeOf<NotFoundException>());
		}

		[Test]
		public void ItShouldFindRootWithoutParentId()
		{
			var artifact1 = new Artifact
			{
				ArtifactID = 1
			};
			var artifact2 = new Artifact
			{
				ArtifactID = 2,
				ParentArtifactID = 1
			};
			var artifact3 = new Artifact
			{
				ArtifactID = 3,
				ParentArtifactID = 2
			};
			var artifact4 = new Artifact
			{
				ArtifactID = 4,
				ParentArtifactID = 1
			};

			var root = _treeByParentIdCreator.Create(new List<Artifact> {artifact1, artifact2, artifact3, artifact4});

			Assert.That(root.Id, Is.EqualTo(artifact1.ArtifactID.ToString()));
		}

		[Test]
		public void ItShouldFindRootWithParentId()
		{
			var artifact1 = new Artifact
			{
				ArtifactID = 1,
				ParentArtifactID = 0
			};
			var artifact2 = new Artifact
			{
				ArtifactID = 2,
				ParentArtifactID = 1
			};
			var artifact3 = new Artifact
			{
				ArtifactID = 3,
				ParentArtifactID = 2
			};
			var artifact4 = new Artifact
			{
				ArtifactID = 4,
				ParentArtifactID = 1
			};

			var root = _treeByParentIdCreator.Create(new List<Artifact> {artifact1, artifact2, artifact3, artifact4});

			Assert.That(root.Id, Is.EqualTo(artifact1.ArtifactID.ToString()));
		}

		[Test]
		public void ItShouldBuildProperTree()
		{
			var artifact1 = new Artifact
			{
				ArtifactID = 1
			};
			var artifact11 = new Artifact
			{
				ArtifactID = 11,
				ParentArtifactID = 1
			};
			var artifact12 = new Artifact
			{
				ArtifactID = 12,
				ParentArtifactID = 1
			};
			var artifact121 = new Artifact
			{
				ArtifactID = 121,
				ParentArtifactID = 12
			};
			var artifact1211 = new Artifact
			{
				ArtifactID = 1211,
				ParentArtifactID = 121
			};

			var artifacts = new List<Artifact>
			{
				artifact1211,
				artifact121,
				artifact12,
				artifact11,
				artifact1
			};

			var treeRoot = _treeByParentIdCreator.Create(artifacts);

			//Assert root
			Assert.That(treeRoot.Id, Is.EqualTo(artifact1.ArtifactID.ToString()));

			//Assert level 2 items
			Assert.That(treeRoot.Children.Count, Is.EqualTo(2));
			Assert.That(treeRoot.Children.Any(x => x.Id.Equals(artifact11.ArtifactID.ToString())));
			Assert.That(treeRoot.Children.Any(x => x.Id.Equals(artifact12.ArtifactID.ToString())));

			//Assert level 3 items
			var treeItem11 = treeRoot.Children.First(x => x.Id.Equals(artifact11.ArtifactID.ToString()));
			Assert.That(treeItem11.Children, Is.Empty);

			var treeItem12 = treeRoot.Children.First(x => x.Id.Equals(artifact12.ArtifactID.ToString()));
			Assert.That(treeItem12.Children.Count, Is.EqualTo(1));
			Assert.That(treeItem12.Children.Any(x => x.Id.Equals(artifact121.ArtifactID.ToString())));

			//Assert level 4 items
			var treeItem121 = treeItem12.Children.First(x => x.Id.Equals(artifact121.ArtifactID.ToString()));
			Assert.That(treeItem121.Children.Count, Is.EqualTo(1));
			Assert.That(treeItem121.Children.Any(x => x.Id.Equals(artifact1211.ArtifactID.ToString())));
		}
	}
}