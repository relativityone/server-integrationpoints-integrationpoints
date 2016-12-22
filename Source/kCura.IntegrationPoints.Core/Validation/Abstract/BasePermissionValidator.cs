using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public abstract class BasePermissionValidator : BasePartsValidator<IntegrationPointProviderValidationModel>, IPermissionValidator
	{
		protected readonly ISerializer _serializer;

		protected BasePermissionValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}
	}
}
