using System;

namespace Relativity.Sync.Tests.Unit
{
	/// <summary>
	///    Contains extension methods for working with <see cref="double"/> values.
	/// </summary>
	public static class DoubleExtensions
	{
		/// <summary>
		///    Determines whether two <see cref="double"/>s are equal within a certain range of error.
		/// </summary>
		/// <param name="me">First double to compare</param>
		/// <param name="you">Second double to compare</param>
		/// <param name="epsilon">Maximum allowed difference (inclusive) between the two values</param>
		/// <returns>True if the two <see cref="double"/> values are within <paramref name="epsilon"/> of each other, false otherwise.</returns>
		public static bool EqualsWithinError(this double me, double you, double epsilon = 1E-15)
		{
			double absEpsilon = Math.Abs(epsilon);
			bool equals = Math.Abs(me - you) <= absEpsilon;
			return equals;
		}
	}
}
