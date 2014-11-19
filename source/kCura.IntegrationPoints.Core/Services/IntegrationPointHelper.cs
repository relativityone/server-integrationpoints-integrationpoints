using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointHelper
	{
		private readonly IServiceContext _context;
		private Data.IntegrationPoint _rdo;
		private Data.IntegrationPoint GetRDO(int rdoID)
		{
			if (_rdo == null)
			{
				_rdo = _context.RsapiService.IntegrationPointLibrary.Read(rdoID);
			}
			return _rdo;
		}

		public IntegrationPointHelper(IServiceContext context)
		{
			_context = context;
		}

		public virtual string GetSourceOptions(int artifactID)
		{
			return GetRDO(artifactID).SourceConfiguration;
		}

		public virtual FieldEntry GetIdentifierFieldEntry(int artifactID)
		{
			var rdo = GetRDO(artifactID);
			var fields = JsonConvert.DeserializeObject<List<FieldMap>>(rdo.FieldMappings);
			return fields.First(x => x.IsIdentifier).SourceField;
		}
		
	}
}
