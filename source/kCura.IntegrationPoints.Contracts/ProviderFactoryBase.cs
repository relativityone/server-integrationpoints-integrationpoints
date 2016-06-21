using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	public abstract class ProviderFactoryBase : IProviderFactory
	{
		public abstract IDataSourceProvider CreateInstance(Type providerType);

		/// <summary>
		/// Creates a new data source provider using the GUID specified as the identifier.
		/// </summary>
		/// <param name="identifier"> A GUID decorating the class so that it can be accessed.</param>
		/// <returns>A new instance of a data source provider.</returns>
		IDataSourceProvider IProviderFactory.CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			return CreateInstance(t);
		}

		/// <summary>
		/// Gets the Type of the current instance.
		/// </summary>
		/// <param name="identifer"></param>
		/// <returns></returns>
		private Type GetType(Guid identifer)
		{
			List<Type> types = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				types.AddRange(loadableTypes.Where(type => type.IsDefined(typeof(DataSourceProviderAttribute), true)));
			}

			var providerTypes = types.Where(x => x.GetCustomAttributes(typeof(DataSourceProviderAttribute), true)
													.Cast<DataSourceProviderAttribute>().Any(y => y.Identifier.Equals(identifer))).ToList();
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
	}
}