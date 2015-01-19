﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Represents the default provider to create data providers.
	/// </summary>
	public class DefaultProviderFactory : IProviderFactory
	{
		public DefaultProviderFactory() { }

		/// <summary>
		/// Creates a new provider based on the identifier guid
		/// </summary>
		/// <param name="identifier">The identifier that should be decorated on the class to access it.</param>
		/// <returns>A new instance of a DataSource provider.</returns>
		public IDataSourceProvider CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			return CreateInstance(t);
		}
		/// <summary>
		/// Gets the type through
		/// </summary>
		/// <param name="identifer"></param>
		/// <returns></returns>
		protected virtual Type GetType(Guid identifer)
		{
			var types = from a in AppDomain.CurrentDomain.GetAssemblies()
									from t in a.GetTypes()
									where t.GetInterface("IDataSourceProvider", true) != null
									select t;
			var attributes = types.SelectMany(x => x.GetCustomAttributes(true)).ToList();
			attributes = attributes.Where(y => y.GetType().Name.Equals(typeof(DataSourceProviderAttribute).Name)).ToList();
			List<DataSourceProviderAttribute> providerTypes = attributes.Select(x => (DataSourceProviderAttribute)x).ToList();
			providerTypes = providerTypes.Where(z => z.Identifier.Equals(identifer)).ToList();
			if (providerTypes.Count() > 1)
			{
				throw new Exception(string.Format(Properties.Resources.MoreThanOneProviderFound, providerTypes.Count(), identifer));
			}
			if (!providerTypes.Any())
			{
				throw new Exception(string.Format(Properties.Resources.NoProvidersFound, identifer));
			}
			return providerTypes.First().GetType();
		}
		/// <summary>
		/// Creates a new instance of the provider type with an empty constructor using Activator
		/// </summary>
		/// <param name="providerType">The type of the provider that will be create.</param>
		/// <returns>A new instance of the Datasource provider.</returns>
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
