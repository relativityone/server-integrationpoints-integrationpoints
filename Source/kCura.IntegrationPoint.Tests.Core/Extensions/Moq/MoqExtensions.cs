using System.Collections.Generic;
using System.Linq;
using Moq.Language;

namespace kCura.IntegrationPoint.Tests.Core.Extensions.Moq
{
	public static class MoqExtensions
	{
		public static ISetupSequentialResult<T> Returns<T>(
			this ISetupSequentialResult<T> setupSequentialResult,
			IEnumerable<T> valuesToReturn)
		{
			return valuesToReturn
				.Aggregate(
					setupSequentialResult, 
					(current, valueToReturn) => current.Returns(valueToReturn));
		}
	}
}
