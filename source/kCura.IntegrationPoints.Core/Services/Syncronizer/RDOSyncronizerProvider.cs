using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public class RDOSyncronizerProvider
	{
		public const string RDO_SYNC_TYPE_GUID = "4380b80b-57ef-48c3-bf02-b98d2855166b";

		private readonly ICaseServiceContext _context;

		public RDOSyncronizerProvider(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual void CreateOrUpdateLdapSourceType()
		{
			var q = new Query<RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, RDO_SYNC_TYPE_GUID);
			var s = _context.RsapiService.DestinationProviderLibrary.Query(q).SingleOrDefault(); //there should only be one!
			if (s == null)
			{
				var rdo = new DestinationProvider();
				rdo.Name = "RDO";
				rdo.Identifier = RDO_SYNC_TYPE_GUID;
				_context.RsapiService.DestinationProviderLibrary.Create(rdo);
			}
			else
			{
				_context.RsapiService.DestinationProviderLibrary.Update(s);
				//edit
			}
		}

		public int GetRdoSyncronizerId()
		{
			var q = new Query<RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, RDO_SYNC_TYPE_GUID);
			var s = _context.RsapiService.DestinationProviderLibrary.Query(q).Single(); //there should only be one!
			return s.ArtifactId;
		}

	}
}
