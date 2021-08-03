using System;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public class TestContext
	{
		private DateTime? _currentDateTime;

		public DateTime CurrentDateTime => _currentDateTime ?? DateTime.UtcNow;

		public FakeUser User { get; set; }

		public InstanceSettings InstanceSettings { get; set; }

		public TestContext()
		{
			InstanceSettings = new InstanceSettings();
		}

		public void SetDateTime(DateTime? dateTime)
		{
			_currentDateTime = dateTime;
		}
	}
}
