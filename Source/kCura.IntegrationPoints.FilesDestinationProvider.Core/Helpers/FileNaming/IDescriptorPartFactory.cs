using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public interface IDescriptorPartFactory
    {
        string Type { get; }
        DescriptorPart Create(FileNamePartModel fileNamePart);
    }
}