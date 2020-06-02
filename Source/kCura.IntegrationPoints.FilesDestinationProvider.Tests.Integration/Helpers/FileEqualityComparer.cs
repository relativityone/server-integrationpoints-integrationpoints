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
		public bool Equals(FileInfo file1, FileInfo file2)
		{
			bool result;
			using(var stream1 = file1.OpenRead())
			using(var stream2 = file2.OpenRead())
			{
				result = MD5.GenerateHashForStream(file1.OpenRead()) == MD5.GenerateHashForStream(file2.OpenRead());
			}

			return result;
		}

		public int GetHashCode(FileInfo obj) => obj.GetHashCode();
	}
}