using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
	public class LdapSourceTypeCreator
	{
		public const string LDAP_SOURCE_TYPE_GUID = "5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232";
		
		private readonly ICaseServiceContext _context;

		public LdapSourceTypeCreator(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual void CreateOrUpdateLdapSourceType()
		{
			var q = new Query<Relativity.Client.DTOs.RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, LDAP_SOURCE_TYPE_GUID);
			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{Data.SourceProviderFields.Identifier}' == '{LDAP_SOURCE_TYPE_GUID}'"
			};
			var s = _context.RsapiService.RelativityObjectManager.Query<SourceProvider>(request).SingleOrDefault(); //there should only be one!
			if (s == null)
			{
				var rdo = new SourceProvider();
				rdo.Name = "LDAP";
				rdo.Identifier = LDAP_SOURCE_TYPE_GUID;
				_context.RsapiService.RelativityObjectManager.Create(rdo);
			}
			else
			{
				s.SourceConfigurationUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/LDAPConfiguration/";
				_context.RsapiService.RelativityObjectManager.Update(s);
				//edit
			}
		}
	}
}
