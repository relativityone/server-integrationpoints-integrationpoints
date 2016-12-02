using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class FolderValidator : BasePartsValidator<int>
	{
		public override ValidationResult Validate(int value)
		{
			// TODO: implement destination folder validation
			return new ValidationResult();
		}
	}
}