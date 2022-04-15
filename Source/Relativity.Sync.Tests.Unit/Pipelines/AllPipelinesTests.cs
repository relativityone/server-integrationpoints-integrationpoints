using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Unit.Pipelines
{
	[TestFixture]
	public class AllPipelinesTests
	{
		[Test]
		public void AllPipelines_ShouldBeSealed()
		{
			// Arrange
			Type interfaceType = typeof(ISyncPipeline);
			Type[] allPipelineTypes = interfaceType.Assembly.GetTypes()
				.Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface).ToArray();

			// Act
			Type[] notSealedTypes = allPipelineTypes.Where(x => !x.IsSealed).ToArray();

			// Assert
			notSealedTypes.Should().BeEmpty("All pipeline types should be sealed");
		}
	}
}
