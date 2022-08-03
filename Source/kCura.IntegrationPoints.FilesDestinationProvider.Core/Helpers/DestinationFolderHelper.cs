

using System;
using System.IO;
using Relativity.DataExchange.Io;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class DestinationFolderHelper : IDestinationFolderHelper
    {
        private readonly IJobInfo _jobInfo;
        private readonly IDirectory _directoryWrap;

        public DestinationFolderHelper(IJobInfo jobInfo, IDirectory directoryWrap)
        {
            _jobInfo = jobInfo;
            _directoryWrap = directoryWrap;
        }

        public string GetFolder(ExportSettings exportSettings)
        {
            string folderPath = exportSettings.ExportFilesLocation;
            if (exportSettings.IsAutomaticFolderCreationEnabled)
            {
                string name = _jobInfo.GetName();
                DateTime startTimeUtc = _jobInfo.GetStartTimeUtc();

                string path = string.Format($"{name}_{startTimeUtc.ToString("s", System.Globalization.CultureInfo.InvariantCulture).Replace(":", "")}");
                folderPath = Path.Combine(folderPath, path);
            }

            return folderPath;
        }

        public void CreateDestinationSubFolderIfNeeded(ExportSettings exportSettings, string path)
        {
            if (exportSettings.IsAutomaticFolderCreationEnabled)
            {
                _directoryWrap.CreateDirectory(path);
            }
        }
    }
}
