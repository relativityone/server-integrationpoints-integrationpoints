using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointReader
	{
		private IRSAPIService _service; 
		public IntegrationPointReader(IRSAPIService service)
		{
			_service = service;

		}

		public IntegrationModel ReadIntegrationPoint(int objectId)
		{

			return new IntegrationModel(_service.IntegrationPointLibrary.Read( objectId));
		}

	}
}
