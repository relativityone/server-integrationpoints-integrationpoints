using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Core.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
	public interface IDescriptorPartFactory
	{
		string Type { get; }
		DescriptorPart Create(FileNamePartModel fileNamePart);
	}
}