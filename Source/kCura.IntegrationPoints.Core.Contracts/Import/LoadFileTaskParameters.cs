using System;

namespace kCura.IntegrationPoints.Core.Contracts.Import
{
	[Serializable]
	public class LoadFileTaskParameters
	{
		public long Size { get; set; }

		public DateTime ModifiedDate { get; set; }
	}
}
