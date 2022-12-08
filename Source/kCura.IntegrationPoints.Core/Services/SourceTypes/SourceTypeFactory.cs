using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
    public class SourceTypeFactory : ISourceTypeFactory
    {
        private readonly IRelativityObjectManager _objectManager;

        public SourceTypeFactory(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public virtual IEnumerable<SourceType> GetSourceTypes()
        {
            var request = new QueryRequest
            {
                Fields = RDOConverter.GetFieldList<Data.SourceProvider>()
            };

            IList<Data.SourceProvider> types = _objectManager.Query<Data.SourceProvider>(request);
            return types
                .Select(ConvertSourceProviderToSourceType)
                .OrderBy(x => x.Name)
                .ToList();
        }

        private static SourceType ConvertSourceProviderToSourceType(Data.SourceProvider sourceProvider)
        {
            return new SourceType
            {
                Name = sourceProvider.Name,
                ID = sourceProvider.Identifier,
                SourceURL = sourceProvider.SourceConfigurationUrl,
                ArtifactID = sourceProvider.ArtifactId,
                Config = sourceProvider.Config
            };
        }
    }
}
