using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Nodes;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncMultiNodeTests
	{
		private SyncMultiNode _instance;

		private const string _CHILD_1_NAME = "child 1";
		private const string _CHILD_2_NAME = "child 2";

		[SetUp]
		public void SetUp()
		{
			INode<SyncExecutionContext> child1 = new FuncNode<SyncExecutionContext>();
			child1.Id = _CHILD_1_NAME;
			INode<SyncExecutionContext> child2 = new FuncNode<SyncExecutionContext>();
			child2.Id = _CHILD_2_NAME;

			_instance = new SyncMultiNode(new SyncExecutionContextFactory(new SyncJobExecutionConfiguration()));
			_instance.AddChild(child1);
			_instance.AddChild(child2);
		}

		[Test]
		public async Task ItShouldReportMergedProgress()
		{
			SyncJobProgressStub syncJobProgressStub = new SyncJobProgressStub();
			SyncExecutionContext context = new SyncExecutionContext(syncJobProgressStub, CompositeCancellationToken.None);

			// ACT
			await _instance.ExecuteAsync(context).ConfigureAwait(false);

			// ASSERT
			syncJobProgressStub.SyncJobState.Id.Should().BeOneOf($"{_CHILD_1_NAME}{Environment.NewLine}{_CHILD_2_NAME}", $"{_CHILD_2_NAME}{Environment.NewLine}{_CHILD_1_NAME}");
		}
	}
}