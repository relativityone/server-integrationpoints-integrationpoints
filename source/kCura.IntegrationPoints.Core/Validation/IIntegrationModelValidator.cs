using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
	public interface IIntegrationModelValidator
	{
		void Validate(IntegrationModel model);
	}
}
