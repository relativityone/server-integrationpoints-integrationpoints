using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Utility;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public class FileEqualityComparer : IEqualityComparer<FileInfo>
	{
		public bool Equals(FileInfo x, FileInfo y)
			=> MD5.GenerateHashForStream(x.OpenRead()) == MD5.GenerateHashForStream(y.OpenRead());

		public int GetHashCode(FileInfo obj) => obj.GetHashCode();
	}
}