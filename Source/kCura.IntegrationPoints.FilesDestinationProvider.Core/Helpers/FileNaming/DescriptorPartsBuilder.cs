using System.Collections.Generic;
using System.Linq;
using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public class DescriptorPartsBuilder : IDescriptorPartsBuilder
    {
        private readonly IDictionary<string, IDescriptorPartFactory> _descriptorPartFactories;

        public DescriptorPartsBuilder(IDictionary<string, IDescriptorPartFactory> descriptorPartFactories)
        {
            _descriptorPartFactories = descriptorPartFactories;
        }

        public List<DescriptorPart> CreateDescriptorParts(IEnumerable<FileNamePartModel> fileNameParts)
        {
            return fileNameParts.Select(part => _descriptorPartFactories[part.Type].Create(part)).ToList();
        }
    }
}
