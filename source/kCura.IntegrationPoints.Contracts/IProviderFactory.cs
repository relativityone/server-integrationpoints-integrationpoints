using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// 
	/// </summary>
	public interface IProviderFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		IDataSourceProvider CreateProvider(Guid identifier);
	}
}
