﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// The abstract class required to inherit from in order to create a Provider factory.
	/// </summary>
	public abstract class ProviderFactoryBase : IProviderFactory
	{
		/// <summary>
		/// Creates an instance of the given Provider.
		/// </summary>
		/// <remarks>If your Provider requires constructor parameters, create a class that implements this method and it will be called an instance of your Provider is needed.</remarks>
		/// <param name="providerType">The type of the Provider to create.</param>
		/// <returns>The concrete class implementing the <see cref="IDataSourceProvider"/> interface.</returns>
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
		/// <param name="identifier">The guid to identify the Provider.</param>
		/// <returns>The type of the Provider.</returns>
		private Type GetType(Guid identifier)
		{
			List<Type> types = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				types.AddRange(loadableTypes.Where(type => type.IsDefined(typeof(DataSourceProviderAttribute), true)));
			}

			var providerTypes = types.Where(x => x.GetCustomAttributes(typeof(DataSourceProviderAttribute), true)
													.Cast<DataSourceProviderAttribute>().Any(y => y.Identifier.Equals(identifier))).ToList();
			if (providerTypes.Count() > 1)
			{
				throw new Exception(string.Format(Properties.Resources.MoreThanOneProviderFound, providerTypes.Count(), identifier));
			}
			if (!providerTypes.Any())
			{
				throw new Exception(string.Format(Properties.Resources.NoProvidersFound, identifier));
			}
			return providerTypes.First();
		}
	}
}