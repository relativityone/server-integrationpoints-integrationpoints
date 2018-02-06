using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Queries
{
	public class GetSourceProviderRdoByIdentifier : GetObjectBase, IGetSourceProviderRdoByIdentifier
	{
		private readonly ICaseServiceContext _context;
		public GetSourceProviderRdoByIdentifier(ICaseServiceContext context)
			: base(typeof(SourceProvider))
		{
			_context = context;
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
			return _context.RsapiService.RelativityObjectManager.Query<SourceProvider>(request).First();
		}
	}
}
