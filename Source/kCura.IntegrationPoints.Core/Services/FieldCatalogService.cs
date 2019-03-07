using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.FieldMapping;

namespace kCura.IntegrationPoints.Core.Services
{
	public class FieldCatalogService : IFieldCatalogService
	{

		private readonly IHelper _helper;
		public FieldCatalogService(IHelper helper)
		{
			_helper = helper;
		}

		public ExternalMapping[] GetAllFieldCatalogMappings(int workspaceId)
		{
			using (IFieldMapping proxy = _helper.GetServicesManager().CreateProxy<IFieldMapping>(ExecutionIdentity.System))
			{
				return proxy.GetAllMappedFieldsAsync(workspaceId, new Guid[0], 0).Result;
			}
		}
	}
}
