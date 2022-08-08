using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public class FieldDescriptorPartFactory : IDescriptorPartFactory
    {
        public string Type => "F";

        public DescriptorPart Create(FileNamePartModel fileNamePart)
        {
            int value = int.Parse(fileNamePart.Value);
            return new FieldDescriptorPart(value);
        }
    }
}