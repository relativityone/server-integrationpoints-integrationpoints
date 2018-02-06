using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Queries
{
	public class GetSourceProviderRdoByApplicationIdentifier : GetObjectBase, IGetSourceProviderRdoByApplicationIdentifier
    {
		private ICaseServiceContext _context;
		public GetSourceProviderRdoByApplicationIdentifier(ICaseServiceContext context)
			: base(typeof(SourceProvider))
		{
			_context = context;
		}

		public List<SourceProvider> Execute(Guid appGuid)
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.SourceProvider)
				},
				Fields = RDOConverter.ConvertPropertiesToFields<SourceProvider>(),
				Condition = $"'{SourceProviderFields.ApplicationIdentifier}' == '{appGuid}'"
			};
			return _context.RsapiService.RelativityObjectManager.Query<SourceProvider>(request);
		}
	}
}
