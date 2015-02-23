using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Tests
{
	public class IntegrationTestAttribute : NUnit.Framework.CategoryAttribute
	{
		public IntegrationTestAttribute() : base("Integration"){}
	}
}
