using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.TagsCreation.SourceWorkspaceTagsCreation;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SourceWorkspaceTagsCreationStepTests
	{
		private SourceWorkspaceTagsCreationStep _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new SourceWorkspaceTagsCreationStep();
		}

		[Test]
		public async Task ItShouldAlwaysCanExecute()
		{
			bool canExecute = await _sut.CanExecuteAsync(It.IsAny<ISourceWorkspaceTagsCreationConfiguration>(), It.IsAny<CancellationToken>()).ConfigureAwait(false);

			Assert.True(canExecute);
		}
	}
}