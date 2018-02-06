using System;
using System.Runtime.Remoting;
using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Domain
{
	public interface IDomainManager
	{
		/// <summary>
		/// Called to initialized the provider's app domain and do any setup work needed
		/// </summary>
		void Init();

		IProviderFactory CreateProviderFactory();
		object GetLifetimeService();
		object InitializeLifetimeService();
		ObjRef CreateObjRef(Type requestedType);
	}
}