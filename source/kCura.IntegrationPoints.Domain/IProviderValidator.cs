using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain
{
	public interface IProviderValidator
	{
		ValidationResult Validate();
	}
}
