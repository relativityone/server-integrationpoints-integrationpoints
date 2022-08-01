using System;
using kCura.IntegrationPoints.Common.Helpers;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    internal class FakeDateTimeWrapper : IDateTime
    {
        private readonly TestContext _context;

        public FakeDateTimeWrapper(TestContext context)
        {
            _context = context;
        }

        public DateTime UtcNow => _context.CurrentDateTime;
    }
}
