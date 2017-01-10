using System;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Helpers
{
	public class PermissionsHelper
	{
		public static void AssertPermissionErrorMessage(Action action)
		{
			Assert.That(action, Throws.TypeOf<AggregateException>().With.InnerException.With.Message.EqualTo("You do not have permission to access this service."));
		}
	}
}