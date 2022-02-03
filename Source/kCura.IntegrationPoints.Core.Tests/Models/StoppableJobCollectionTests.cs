using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Models
{
	[TestFixture, Category("Unit")]
	public class StoppableJobCollectionTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void HasStoppableJobs_NullProperties_ReturnsFalse()
		{
			// Arrange
			var instance = new StoppableJobHistoryCollection();

			// Act & Assert
			Assert.IsFalse(instance.HasStoppableJobHistory);
		}

		[Test]
		public void HasStoppableJobs_EmptyProperties_ReturnsFalse()
		{
			// Arrange
			var instance = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new Data.JobHistory[0],
				ProcessingJobHistory = new Data.JobHistory[0]
			};

			// Act & Assert
			Assert.IsFalse(instance.HasStoppableJobHistory);
		}

		[Test]
		public void HasStoppableJobs_GoldFlow()
		{
			// Arrange
			var instance = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new [] { new Data.JobHistory { ArtifactId = 231 } },
				ProcessingJobHistory = new [] { new Data.JobHistory { ArtifactId = 95403 } }
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobHistory);
		}

		[Test]
		public void HasStoppableJobs_NoPendingJobs_ReturnsTrue()
		{
			// Arrange
			var instance = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = null,
				ProcessingJobHistory = new[] { new Data.JobHistory { ArtifactId = 95403 } }
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobHistory);
		}

		[Test]
		public void HasStoppableJobs_NoProcessingJobs_ReturnsTrue()
		{
			// Arrange
			var instance = new StoppableJobHistoryCollection()
			{
				PendingJobHistory = new[] { new Data.JobHistory { ArtifactId = 904302 } },
				ProcessingJobHistory = null
			};

			// Act & Assert
			Assert.IsTrue(instance.HasStoppableJobHistory);
		}
	}
}