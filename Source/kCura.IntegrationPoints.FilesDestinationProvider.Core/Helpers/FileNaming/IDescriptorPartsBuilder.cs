using System.Collections.Generic;
using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public interface IDescriptorPartsBuilder
    {
        List<DescriptorPart> CreateDescriptorParts(IEnumerable<FileNamePartModel> fileNameParts);
    }
}