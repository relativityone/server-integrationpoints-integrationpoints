using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class RdoSynchronizerProvider : IRdoSynchronizerProvider
	{
		public const string RDO_SYNC_TYPE_GUID = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
		public const string FILES_SYNC_TYPE_GUID = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

		private readonly ICaseServiceContext _context;

		public RdoSynchronizerProvider(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual void CreateOrUpdateDestinationProviders()
		{
			CreateOrUpdateDestinationProvider("Relativity", RDO_SYNC_TYPE_GUID);
			CreateOrUpdateDestinationProvider("Load File", FILES_SYNC_TYPE_GUID);

			//var q = new Query<Relativity.Client.DTOs.RDO>();
			//q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, RDO_SYNC_TYPE_GUID);
			//var s = _context.RsapiService.DestinationProviderLibrary.Query(q).SingleOrDefault(); //there should only be one!
			//if (s == null)
			//{
			//	var rdo = new DestinationProvider();
			//	rdo.Name = "RDO";
			//	rdo.Identifier = RDO_SYNC_TYPE_GUID;
			//	rdo.ApplicationIdentifier = Application.GUID;
			//	_context.RsapiService.DestinationProviderLibrary.Create(rdo);
			//}
			//else
			//{
			//	_context.RsapiService.DestinationProviderLibrary.Update(s);
			//	//edit
			//}
		}

		private void CreateOrUpdateDestinationProvider(string name, string providerGuid)
		{
			var q = new Query<RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, providerGuid);
			var s = _context.RsapiService.DestinationProviderLibrary.Query(q).SingleOrDefault(); //there should only be one!
			if (s == null)
			{
				var rdo = new DestinationProvider();
				rdo.Name = name;
				rdo.Identifier = providerGuid;
				rdo.ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING;
				_context.RsapiService.DestinationProviderLibrary.Create(rdo);
			}
			else
			{
				_context.RsapiService.DestinationProviderLibrary.Update(s);
				//edit
			}
		}

		public int GetRdoSynchronizerId()
		{
			var q = new Query<Relativity.Client.DTOs.RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, RDO_SYNC_TYPE_GUID);
			var s = _context.RsapiService.DestinationProviderLibrary.Query(q).Single(); //there should only be one!
			return s.ArtifactId;
		}
	}
}
