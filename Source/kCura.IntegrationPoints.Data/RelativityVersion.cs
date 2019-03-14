using System;

namespace kCura.IntegrationPoints.Data
{
	public static class RelativityVersion
	{
		private static Version _currentRelativityVersion = null;
		public static Version GetCurrentVersion()
		{
			if (_currentRelativityVersion == null)
			{
				_currentRelativityVersion = typeof(kCura.Relativity.Client.RSAPIClient).Assembly.GetName().Version;
			}
			return _currentRelativityVersion;
		}

		private static bool? _isRelativityVersion93OrGreater = null;
		public static bool IsRelativityVersion93OrGreater
		{
			get
			{
				if (!_isRelativityVersion93OrGreater.HasValue)
				{
					Version currentRelativityVersion = GetCurrentVersion();
					Version version = new Version(9, 3);
					if (version.CompareTo(currentRelativityVersion) > 0)
					{
						_isRelativityVersion93OrGreater = false;
					}
					else
					{
						_isRelativityVersion93OrGreater = true;
					}
				}
				return _isRelativityVersion93OrGreater.Value;
			}
		}
	}
}
