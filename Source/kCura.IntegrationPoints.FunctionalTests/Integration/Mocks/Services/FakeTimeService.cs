using System;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeTimeService : ITimeService
    {
        private readonly TestContext _context;

        public DateTime UtcNow => _context.CurrentDateTime;

        public DateTime LocalTime => UtcNow.ToLocalTime();

        public FakeTimeService(TestContext context)
        {
            _context = context;
        }
    }
}
