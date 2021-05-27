using System;

namespace kCura.IntegrationPoints.ImportProvider
{
	[Serializable]
	public class LoadFileTaskParameters
	{
		public long Size { get; }

		public DateTime ModifiedDate { get; }

		public LoadFileTaskParameters(long fileSize, DateTime modifiedDate)
		{
			Size = fileSize;
			ModifiedDate = modifiedDate;
		}
	}
}
