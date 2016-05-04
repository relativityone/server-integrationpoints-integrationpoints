using Castle.Windsor;

namespace kCura.IntegrationPoints.Services
{
	public interface IRequiredPermission
	{
		/// <summary>
		/// Validate permission of the properties against the user who made the request.
		/// </summary>
		/// <param name="container">Castle Windsor container to resolve dependencies.</param>
		void ValidatePermission(IWindsorContainer container);
	}
}