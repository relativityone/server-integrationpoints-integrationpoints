using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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
			var query = new Query<RDO>();
			query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.SourceProvider);
			query.Condition = new TextCondition(Guid.Parse(SourceProviderFieldGuids.Identifier),
																								 TextConditionEnum.EqualTo, providerGuid.ToString());

			query.Fields = base.GetFields();

			return _context.RsapiService.SourceProviderLibrary.Query(query).First();
		}
	}
}
