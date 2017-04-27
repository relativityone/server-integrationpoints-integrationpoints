using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Core.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
	public class SeparatorPartDescriptorFactory : IDescriptorPartFactory
	{
		public string Type => "S";

		public DescriptorPart Create(FileNamePartModel fileNamePart)
		{
			return new SeparatorDescriptorPart(fileNamePart.Value);
		}
	}
}