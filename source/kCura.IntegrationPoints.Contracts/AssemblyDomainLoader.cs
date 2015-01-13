using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	public class AssemblyDomainLoader : MarshalByRefObject
	{
		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name="rawAssembly">the library that will be loaded into the current Application Domain.</param>
		public void Load(byte[] rawAssembly)
		{
			if (rawAssembly == null)
			{
				throw new ArgumentNullException("rawAssembly");
			}
			Assembly.Load(rawAssembly);
		}

	}
}
