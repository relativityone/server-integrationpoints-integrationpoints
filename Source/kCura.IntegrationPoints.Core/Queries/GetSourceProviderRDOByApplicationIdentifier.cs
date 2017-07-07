using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Queries
{
	public class GetSourceProviderRdoByApplicationIdentifier : GetObjectBase, IGetSourceProviderRdoByApplicationIdentifier
    {
		private ICaseServiceContext _context;
		public GetSourceProviderRdoByApplicationIdentifier(ICaseServiceContext context)
			: base(typeof(Data.SourceProvider))
		{
			_context = context;
		}

		public List<Data.SourceProvider> Execute(Guid appGuid)
		{
			var query = new Query<RDO>();
			query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.SourceProvider);
			query.Condition = new TextCondition(Guid.Parse(SourceProviderFieldGuids.ApplicationIdentifier),
																								 TextConditionEnum.EqualTo, appGuid.ToString());

			query.Fields = base.GetFields();

			return _context.RsapiService.SourceProviderLibrary.Query(query);
		}
	}
}
