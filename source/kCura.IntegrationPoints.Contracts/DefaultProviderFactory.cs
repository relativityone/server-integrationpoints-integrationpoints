using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	public class DefaultProviderFactory : IProviderFactory
	{
		public IDataSourceProvider CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			return CreateInstance(t);
		}
		
		protected virtual Type GetType(Guid identifer)
		{
			var types = from a in AppDomain.CurrentDomain.GetAssemblies()
									from t in a.GetTypes()
									where t.IsDefined(typeof(DataSourceProviderAttribute), true)
									select t;
			var providerTypes = types.Where(x => x.GetCustomAttributes(typeof(DataSourceProviderAttribute), true).Cast<DataSourceProviderAttribute>().Any(y => y.Identifier.Equals(identifer))).ToList();
			if (providerTypes.Count() > 1)
			{
				throw new Exception(string.Format(Properties.Resources.MoreThanOneProviderFound, providerTypes.Count(), identifer));
			}
			if (!providerTypes.Any())
			{
				throw new Exception(string.Format(Properties.Resources.NoProvidersFound, identifer));
			}
			return providerTypes.First();
		}

		protected virtual IDataSourceProvider CreateInstance(Type providerType)
		{
			var provider = Activator.CreateInstance(providerType) as IDataSourceProvider;
			if (provider == null)
			{
				throw new Exception(string.Format(Properties.Resources.CouldNotCreateProvider, providerType));
			}
			return provider;
		}
	}
}
