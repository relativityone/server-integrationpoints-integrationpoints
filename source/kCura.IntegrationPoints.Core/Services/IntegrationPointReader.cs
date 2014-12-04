using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointReader
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

		public IntegrationPointReader(IServiceContext context)
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

		public IntegrationModel ReadIntegrationPoint(int objectId)
		{
			return new IntegrationModel(_context.RsapiService.IntegrationPointLibrary.Read(objectId));
		}

		public IEnumerable<FieldMap> GetFieldMap(int objectId)
		{
			var fieldmapjson= _context.RsapiService.IntegrationPointLibrary.Read(objectId, new Guid(Data.IntegrationPointFieldGuids.FieldMappings)).FieldMappings;
			var fieldmap = Json.Decode<IEnumerable<FieldMap>>(fieldmapjson);
			return fieldmap;

		} 

	}
}
