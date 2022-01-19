using System;
using System.Collections.Generic;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public class TestContext
	{
		private DateTime? _currentDateTime;

		public DateTime CurrentDateTime => _currentDateTime ?? DateTime.UtcNow;

		public FakeUser User { get; set; }

		public InstanceSettings InstanceSettings { get; set; }

		public ToggleValues ToggleValues { get; }

		public TestContext()
		{
			InstanceSettings = new InstanceSettings();
			ToggleValues = new ToggleValues();
		}

		public void SetDateTime(DateTime? dateTime)
		{
			_currentDateTime = dateTime;
		}
	}
}
