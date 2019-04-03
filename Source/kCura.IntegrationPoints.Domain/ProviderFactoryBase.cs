using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Wrappers;

namespace kCura.IntegrationPoints.Domain
{
	/// <summary>
	/// The abstract class from which all provider factory classes are derived.
	/// </summary>
	public abstract class ProviderFactoryBase : MarshalByRefObject, IProviderFactory // TODO move to kCura.IntegrationPoints.Contracts
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
		IDataSourceProvider IProviderFactory.CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			IDataSourceProvider provider = CreateInstance(t);
			return new CrossAppDomainProviderWrapper(provider);
		}

		/// <summary>
		/// Gets the Type of the current instance.
		/// </summary>
		/// <param name="identifier">The guid to identify the Provider.</param>
		/// <returns>The type of the Provider.</returns>
		private Type GetType(Guid identifier)
		{
			List<Type> typesWithAttribute = GetTypesWithDataSourceProviderAttribute();
			List<Type> providerTypes = typesWithAttribute.Where(x => x.GetCustomAttributes(typeof(DataSourceProviderAttribute), true)
													.Cast<DataSourceProviderAttribute>().Any(y => y.Identifier.Equals(identifier))).ToList();
			if (providerTypes.Count > 1)
			{
				throw new IntegrationPointsException(string.Format(Contracts.Properties.Resources.MoreThanOneProviderFound, providerTypes.Count, identifier))
				{ExceptionSource = IntegrationPointsExceptionSource.EVENT_HANDLER};
			}
			if (providerTypes.Count == 0)
			{
				throw new IntegrationPointsException(string.Format(Contracts.Properties.Resources.NoProvidersFound, identifier)) { ExceptionSource = IntegrationPointsExceptionSource.EVENT_HANDLER };
			}
			return providerTypes.First();
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Returns null to ensure infinite object lifetime</returns>
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}