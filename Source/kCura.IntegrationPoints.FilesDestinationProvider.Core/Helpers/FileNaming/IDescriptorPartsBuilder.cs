using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Core.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
	public interface IDescriptorPartsBuilder
	{
		List<DescriptorPart> CreateDescriptorParts(IEnumerable<FileNamePartModel> fileNameParts);
	}
}