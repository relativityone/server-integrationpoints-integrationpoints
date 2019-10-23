using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals.Exceptions;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace Relativity.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// The abstract class from which all provider factory classes are derived.
	/// </summary>
	public abstract class ProviderFactoryBase : MarshalByRefObject, IProviderFactory
	{
		private List<Type> _typesWithDataSourceProviderAttribute;

		/// <summary>
		/// Creates an instance of a provider.
		/// </summary>
		/// <remarks>If the constructor for your provider requires parameters, you must create a class that implements this method. 
		/// The Integration Points framework will call this method when an instance of your provider is needed.</remarks>
		/// <param name="providerType">The type of the provider to create.</param>
		/// <returns>The concrete class implementing the <see cref="IDataSourceProvider"/> interface.</returns>
		public abstract IDataSourceProvider CreateInstance(Type providerType);

		/// <summary>
		/// Creates a new data source provider using the GUID specified as the identifier.
		/// </summary>
		/// <param name="identifier"> A GUID decorating the class so that it can be accessed.</param>
		/// <returns>A new instance of a data source provider.</returns>
		public IDataSourceProvider CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			IDataSourceProvider provider = CreateInstance(t);
			return new CrossAppDomainProviderWrapper(provider);
		}

		/// <summary>
		/// This method should not be overriden by derived class.
		/// </summary>
		/// <returns>Returns null to ensure infinite object lifetime</returns>
		public override object InitializeLifetimeService()
		{
			return null;
		}

		/// <summary>
		/// Gets the Type of the current instance.
		/// </summary>
		/// <param name="identifier">The guid to identify the Provider.</param>
		/// <returns>The type of the Provider.</returns>
		private Type GetType(Guid identifier)
		{
			List<Type> typesWithAttribute = GetTypesWithDataSourceProviderAttribute();
			List<Type> providerTypes = typesWithAttribute.Where(GetIsAttributeIdentifierEqualPredicate(identifier)).ToList();

			if (providerTypes.Count > 1)
			{
				throw new TooManyProvidersFoundException(providerTypes.Count, identifier);
			}
			if (providerTypes.Count == 0)
			{
				throw new NoProvidersFoundException(identifier);
			}
			return providerTypes.Single();
		}

		private static Func<Type, bool> GetIsAttributeIdentifierEqualPredicate(Guid expectedIdentifier)
		{
			return type => type
				.GetCustomAttributes(typeof(DataSourceProviderAttribute), inherit: true)
				.Cast<DataSourceProviderAttribute>()
				.Any(attribute => attribute.Identifier.Equals(expectedIdentifier));
		}

		private List<Type> GetTypesWithDataSourceProviderAttribute()
		{
			if (_typesWithDataSourceProviderAttribute == null)
			{
				_typesWithDataSourceProviderAttribute = CreateTypesWithDataSourceProviderAttributeList();
			}
			return _typesWithDataSourceProviderAttribute;
		}

		private List<Type> CreateTypesWithDataSourceProviderAttributeList()
		{
			var typesWithAttribute = new List<Type>();
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				typesWithAttribute.AddRange(loadableTypes.Where(type =>
					type.IsDefined(typeof(DataSourceProviderAttribute), true)));
			}

			return typesWithAttribute;
		}
	}
}