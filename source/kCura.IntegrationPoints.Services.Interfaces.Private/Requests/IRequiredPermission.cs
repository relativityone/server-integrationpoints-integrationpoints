using Castle.Windsor;

namespace kCura.IntegrationPoints.Services
{
	public interface IRequiredPermission
	{
		void ValidatePermission(IWindsorContainer container);
	}
}