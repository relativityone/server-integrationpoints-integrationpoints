using System;

namespace kCura.Method.Data.Utility
{
	public class RelativityVersion
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

		private static bool? _isRelativityVersion92OrGreater = null;
		public static bool IsRelativityVersion92OrGreater
		{
			get
			{
				if (!_isRelativityVersion92OrGreater.HasValue)
				{
					Version currentRelativityVersion = GetCurrentVersion();
					Version version = new Version(9, 2, 500, 0);
					if (version.CompareTo(currentRelativityVersion) > 0)
					{
						_isRelativityVersion92OrGreater = false;
					}
					else
					{
						_isRelativityVersion92OrGreater = true;
					}
				}
				return _isRelativityVersion92OrGreater.Value;
			}
		}
	}
}
