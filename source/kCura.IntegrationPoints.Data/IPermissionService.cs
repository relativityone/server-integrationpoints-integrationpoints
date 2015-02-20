using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IPermissionService 
	{
		Boolean userCanImport(int userId);
	}
}
