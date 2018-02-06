using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain
{
	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// Internal use only:
	/// This class should not be referenced by any consuming library
	///  </summary>
	public class DomainManager : MarshalByRefObject, IDomainManager
	{
		private IProviderFactory _customProviderFactory;
		private readonly WindsorContainerSetup _windsorContainerSetup;
		private readonly IHelper _helper;

		public DomainManager(IHelper helper)
		{
			_helper = helper;
			_windsorContainerSetup = new WindsorContainerSetup();
		}

		/// <summary>
		/// Called to initialized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			CreateCustomProviderFactory();

			// Get marshaled data
			IAppDomainDataMarshaller dataMarshaller = new SecureAppDomainDataMarshaller();
			this.SetUpConnectionString(dataMarshaller);
		}

		private void CreateCustomProviderFactory()
		{
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type startupType = typeof(IStartUp);
			Type providerFactoryType = typeof(IProviderFactory);

			var startupTypes = new List<Type>();
			Type customProviderFactoryType = null;
			foreach (var assembly in assemblies)
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				foreach (Type type in loadableTypes)
				{
					if (startupType.IsAssignableFrom(type) && type != startupType)
					{
						startupTypes.Add(type);
					}
					else if (providerFactoryType.IsAssignableFrom(type)
							&& type != typeof(DefaultProviderFactory)
							&& type != typeof(ProviderFactoryBase)
							&& type != typeof(InternalProviderFactory)
							&& type != providerFactoryType)
					{
						if (customProviderFactoryType != null)
						{
							throw new Exception(Constants.IntegrationPoints.TOO_MANY_PROVIDER_FACTORIES);
						}

						customProviderFactoryType = type;
					}
				}

				startupTypes.AddRange(loadableTypes.Where(type => startupType.IsAssignableFrom(type) && type != startupType));
			}

			if (startupTypes.Any())
			{
				Type type = startupTypes.FirstOrDefault();
				if (type != default(Type))
				{
					var instance = Activator.CreateInstance(type) as IStartUp;
					instance?.Execute();
				}
			}

			if (customProviderFactoryType != null)
			{
				try
				{
					var customProviderFactory = Activator.CreateInstance(customProviderFactoryType) as IProviderFactory;
					_customProviderFactory = customProviderFactory;
				}
				catch (Exception ex)
				{
					throw new Exception(Constants.IntegrationPoints.UNABLE_TO_INSTANTIATE_PROVIDER_FACTORY, ex);
				}
			}
		}

		/// <summary>
		/// Sets the connection string for the domain by retrieving the encrypted AppDomain data
		/// </summary>
		private void SetUpConnectionString(IAppDomainDataMarshaller dataMarshaller)
		{
			byte[] data = dataMarshaller.RetrieveMarshaledData(AppDomain.CurrentDomain, Constants.IntegrationPoints.APP_DOMAIN_DATA_CONNECTION_STRING);
			if (data != null && data.Length > 0)
			{
				string connectionString = System.Text.Encoding.ASCII.GetString(data);

				kCura.Config.Config.SetConnectionString(connectionString);
			}
		}

		public IProviderFactory CreateProviderFactory()
		{
			IWindsorContainer container = _windsorContainerSetup.SetUpCastleWindsor(_helper);
			IProviderFactory providerFactory = _customProviderFactory;
			if (_customProviderFactory == null)
			{
				try
				{
					providerFactory = container.Resolve<IProviderFactory>();
				}
				catch
				{
					providerFactory = new DefaultProviderFactory(container, _helper);
				}
			}

			return providerFactory;
		}
	}
}