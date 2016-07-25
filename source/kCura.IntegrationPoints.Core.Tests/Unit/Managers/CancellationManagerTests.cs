using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class CancellationManagerTests
	{
		private IRepositoryFactory _repoFactory;
		private JobHistory _jobHistory;
		private Job _job;

		private CancellationManager _instance
			;

		[SetUp]
		public void Setup()
		{
			_repoFactory = NSubstitute.Substitute.For<IRepositoryFactory>();
		}

		[Test]
		public void IsCancellationRequested_JobIsNotYetCanceled()
		{
			// arrange
			_jobHistory = new JobHistory();
			_job = JobExtensions.CreateJob();
			_instance = new CancellationManager(_repoFactory, _jobHistory, null);

			// act
			_instance.Callback.Invoke(null); // make sure to invoke the callback
			bool isCancled = _instance.IsCancellationRequested();

			// assert
			Assert.IsFalse(isCancled);
		}

		[Test]
		public void Dispose_CorrectDisposePattern()
		{
			// arrange
			_jobHistory = new JobHistory();
			_job = JobExtensions.CreateJob();
			_instance = new CancellationManager(_repoFactory, _jobHistory, null);

			// act & assert
			Assert.DoesNotThrow(() => _instance.Dispose());
			Assert.DoesNotThrow(() => _instance.Dispose());
		}
	}
}