using System;

namespace kCura.IntegrationPoints.Domain.Managers
{
    public interface IInstanceSettingsManager
    {
        string RetriveCurrentInstanceFriendlyName();

        bool RetrieveAllowNoSnapshotImport();

        bool RetrieveRestrictReferentialFileLinksOnImport();

        string RetrieveBlockedIPs();

        TimeSpan GetDrainStopTimeout();

        string GetWorkloadSizeSettings();
    }
}