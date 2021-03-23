using System;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public class TestContext
	{
		private DateTime? _currentDateTime;

		public DateTime CurrentDateTime => _currentDateTime ?? DateTime.UtcNow;

		public int UserId { get; set; } = 9;

		public void SetDateTime(DateTime? dateTime)
		{
			_currentDateTime = dateTime;
		}
	}
}
