using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Queries
{
    public class GetSourceProviderRdoByIdentifier : IGetSourceProviderRdoByIdentifier
    {
        private readonly IRelativityObjectManager _objectManager;

        public GetSourceProviderRdoByIdentifier(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public SourceProvider Execute(Guid providerGuid)
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = Guid.Parse(ObjectTypeGuids.SourceProvider)
                },
                Fields = RDOConverter.ConvertPropertiesToFields<SourceProvider>(),
                Condition = $"'{SourceProviderFields.Identifier}' == '{providerGuid}'"
            };
            return _objectManager.Query<SourceProvider>(request).First();
        }
    }
}
