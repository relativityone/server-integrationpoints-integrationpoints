using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
	public class LdapSourceTypeCreator
	{
		public const string LDAP_SOURCE_TYPE_GUID = "5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232";
		
		private readonly IServiceContext _context;

		public LdapSourceTypeCreator(IServiceContext context)
		{
			_context = context;
		}

		public virtual void CreateOrUpdateLdapSourceType()
		{
			var q = new Query<RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, LDAP_SOURCE_TYPE_GUID);
			var s = _context.RsapiService.SourceProviderLibrary.Query(q).SingleOrDefault(); //there should only be one!
			if (s == null)
			{
				var rdo = new SourceProvider();
				rdo.Name = "LDAP";
				rdo.Identifier = LDAP_SOURCE_TYPE_GUID;
				_context.RsapiService.SourceProviderLibrary.Create(rdo);
			}
			else
			{
				//edit
			}
		}
	}
}
