using System;
using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Models
{
    public class IntegrationPointProviderValidationModel
    {
        public IntegrationPointProviderValidationModel()
        {
        }

        public IntegrationPointProviderValidationModel(IntegrationPointDtoBase model)
        {
            FieldsMap = model.FieldMappings;
            SourceConfiguration = model.SourceConfiguration;
            DestinationConfiguration = model.DestinationConfiguration;
            SourceProviderArtifactId = model.SourceProvider;
            DestinationProviderArtifactId = model.DestinationProvider;
            Type = model.Type;
        }

        public int ArtifactId { get; set; }

        public int ArtifactTypeId { get; set; }

        public int UserId { get; set; }

        public int Type { get; set; }

        public List<FieldMap> FieldsMap { get; set; }

        public string SourceProviderIdentifier { get; set; }

        public int SourceProviderArtifactId { get; set; }

        public string SourceConfiguration { get; set; }

        public string DestinationProviderIdentifier { get; set; }

        public int DestinationProviderArtifactId { get; set; }

        public string DestinationConfiguration { get; set; }

        public string IntegrationPointTypeIdentifier { get; set; }

        public Guid ObjectTypeGuid { get; set; }

        public string SecuredConfiguration { get; set; }

        public bool CreateSavedSearch { get; set; } = false;
    }
}
