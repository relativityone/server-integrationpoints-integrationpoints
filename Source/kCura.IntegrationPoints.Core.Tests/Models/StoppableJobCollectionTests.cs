using kCura.IntegrationPoints.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Models
{
	[TestFixture]
	public class StoppableJobCollectionTests
	{
		[Test]
		public void HasStoppableJobs_NullProperties_ReturnsFalse()
		{
			// Arrange
			var instance = new StoppableJobCollection();

			// Act & Assert
			Assert.IsFalse(instance.HasStoppableJobs);
		}

		[Test]
		public void HasStoppableJobs_EmptyProperties_ReturnsFalse()
		{
			// Arrange
			var instance = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new int[0],
				ProcessingJobArtifactIds = new int[0]
			};

			// Act & Assert
			Assert.IsFalse(instance.HasStoppableJobs);
		}

		[Test]
		public void HasStoppableJobs_GoldFlow()
		{
			// Arrange
			var instance = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new [] {231},
				ProcessingJobArtifactIds = new [] {95403}
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobs);
		}

		[Test]
		public void HasStoppableJobs_NoPendingJobs_ReturnsTrue()
		{
			// Arrange
			var instance = new StoppableJobCollection()
			{
				PendingJobArtifactIds = null,
				ProcessingJobArtifactIds = new[] { 95403 }
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobs);
		}

		[Test]
		public void HasStoppableJobs_NoProcessingJobs_ReturnsTrue()
		{
			// Arrange
			var instance = new StoppableJobCollection()
			{
				PendingJobArtifactIds = new [] {904302},
				ProcessingJobArtifactIds = null
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobs);
		}
	}
}