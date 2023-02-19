using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;

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
