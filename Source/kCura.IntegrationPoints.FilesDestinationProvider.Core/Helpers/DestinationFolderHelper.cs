

using System;
using System.IO;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public class DestinationFolderHelper : IDestinationFolderHelper
	{
		private readonly IJobInfo _jobInfo;
		private readonly IDirectoryHelper _dirHelper;

		public DestinationFolderHelper(IJobInfo jobInfo, IDirectoryHelper dirHelper)
		{
			_jobInfo = jobInfo;
			_dirHelper = dirHelper;
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

		public void CreateFolder(string path)
		{
			_dirHelper.CreateDirectory(path);
		}
	}
}
