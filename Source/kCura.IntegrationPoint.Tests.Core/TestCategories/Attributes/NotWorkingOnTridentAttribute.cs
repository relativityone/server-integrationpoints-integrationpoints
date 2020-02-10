using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class NotWorkingOnTridentAttribute : CategoryAttribute
	{
		public NotWorkingOnTridentAttribute() : base(TestCategories.NOT_WORKING_ON_TRIDENT)
		{
		}
	}
}
