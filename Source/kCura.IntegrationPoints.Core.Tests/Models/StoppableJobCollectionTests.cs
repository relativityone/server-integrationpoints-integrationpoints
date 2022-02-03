using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Models
{
	[TestFixture, Category("Unit")]
	public class StoppableJobCollectionTests : TestBase
	{
		[Test]
		public void StoppableJobCollection_ShouldReturn_WhenNullProperties()
		{
			// Arrange
			var sut = new StoppableJobHistoryCollection();

			// Act & Assert
			sut.HasStoppableJobHistory.Should().BeFalse();
			sut.HasOnlyPendingJobHistory.Should().BeFalse();
		}

		[Test]
		public void StoppableJobCollection_ShouldReturn_WhenEmptyProperties()
		{
			// Arrange
			var sut = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new Data.JobHistory[0],
				ProcessingJobHistory = new Data.JobHistory[0]
			};

			// Act & Assert
			sut.HasStoppableJobHistory.Should().BeFalse();
			sut.HasOnlyPendingJobHistory.Should().BeFalse();
		}

		[Test]
		public void StoppableJobCollection_GoldFlow()
		{
			// Arrange
			var sut = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new [] { new Data.JobHistory { ArtifactId = 231 } },
				ProcessingJobHistory = new [] { new Data.JobHistory { ArtifactId = 95403 } }
			};

			// Act & Assert
			sut.HasStoppableJobHistory.Should().BeTrue();
			sut.HasOnlyPendingJobHistory.Should().BeFalse();
		}

		[Test]
		public void StoppableJobCollection_ShouldReturn_WhenNoPendingJobs()
		{
			// Arrange
			var sut = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = null,
				ProcessingJobHistory = new[] { new Data.JobHistory { ArtifactId = 95403 } }
			};

			// Act & Assert
			sut.HasStoppableJobHistory.Should().BeTrue();
			sut.HasOnlyPendingJobHistory.Should().BeFalse();
		}

		[Test]
		public void StoppableJobCollection_ShouldReturne_WhenNoProcessingJobs()
		{
			// Arrange
			var sut = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { new Data.JobHistory { ArtifactId = 904302 } },
				ProcessingJobHistory = null
			};

			// Act & Assert
			sut.HasStoppableJobHistory.Should().BeTrue();
			sut.HasOnlyPendingJobHistory.Should().BeTrue();
		}
	}
}