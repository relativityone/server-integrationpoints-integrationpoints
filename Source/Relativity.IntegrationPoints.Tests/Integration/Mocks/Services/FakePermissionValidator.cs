using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakePermissionValidator : IPermissionValidator
	{
		public string Key { get; }
		public ValidationResult Validate(object value)
		{
			return new ValidationResult(true);
		}
	}
}