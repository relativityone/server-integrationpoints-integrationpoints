using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// Internal use only:
	/// This class should not be referenced by any consuming library
	///  </summary>
	public class DomainManager : MarshalByRefObject
	{
		/// <summary>
		/// Called to initilized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			var startupType = typeof(IStartUp);
			var types = (from a in AppDomain.CurrentDomain.GetAssemblies()
									 from t in a.GetTypes()
									 where startupType.IsAssignableFrom(t)
									 select t).ToList();
			if (types.Any())
			{
				var type = types.FirstOrDefault();
				if (type != default (Type))
				{
					var instance = Activator.CreateInstance(type) as IStartUp;
					if (instance != null)
					{
						instance.Execute();
					}	
				}
			}
		}

		/// <summary>
		/// Gets the provider in the app domain from the specific identifier
		/// </summary>
		/// <param name="identifer">The identifier that represents the provider</param>
		/// <returns>A Data source provider to retrieve data and pass along to the source.</returns>
		public Provider.IDataSourceProvider GetProvider(Guid identifer)
		{
			return ProviderBuilder.Current.GetFactory().CreateProvider(identifer);
		}

	}
}
