using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Queries
{
	public class GetSourceProviderRdoByIdentifier : GetObjectBase, IGetSourceProviderRdoByIdentifier
    {
		private ICaseServiceContext _context;
		public GetSourceProviderRdoByIdentifier(ICaseServiceContext context)
			: base(typeof(Data.SourceProvider))
		{
			_context = context;
		}

		public Data.SourceProvider Execute(Guid providerGuid)
		{
			//var query = new Query<RDO>();
			//query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.SourceProvider);
			//query.Condition = new TextCondition(Guid.Parse(SourceProviderFieldGuids.Identifier),
			//																					 TextConditionEnum.EqualTo, providerGuid.ToString());

			//query.Fields = base.GetFields();
			QueryRequest request = new QueryRequest()
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
