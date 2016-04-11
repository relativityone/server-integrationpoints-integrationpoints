using System;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class DocumentVolumeModel
	{
		public DateTime Date { get; set; }
		public int TotalDocumentsIncluded { get; set; }
		public int TotalDocumentsExcluded { get; set; }
		public int TotalDocumentsUntagged { get; set; }
	} 
}
