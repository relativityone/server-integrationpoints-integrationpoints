using System;
using kCura.IntegrationPoints.Domain.Managers;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    internal class FakeInstanceSettingsManager : IInstanceSettingsManager
    {
        public string RetriveCurrentInstanceFriendlyName()
        {
            throw new NotImplementedException();
        }

        public bool RetrieveAllowNoSnapshotImport()
        {
            throw new NotImplementedException();
        }

        public bool RetrieveRestrictReferentialFileLinksOnImport()
        {
            throw new NotImplementedException();
        }

        public string RetrieveBlockedIPs()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetDrainStopTimeout()
        {
            throw new NotImplementedException();
        }

        public string GetWorkloadSizeSettings()
        {
            throw new NotImplementedException();
        }

        public int GetIApiBatchSize()
        {
            return 1000;
        }
    }
}
