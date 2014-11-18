using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

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

		public FieldEntry GetIdentifierFieldEntry(int artifactID)
		{
			throw new NotImplementedException();
		}
		
	}
}
