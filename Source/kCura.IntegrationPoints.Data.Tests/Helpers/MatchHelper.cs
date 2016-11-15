using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Tests.Helpers
{
	public class MatchHelper
	{
		public static bool Matches<T>(T expected, T actual)
		{
			if (expected != null && actual == null)
			{
				return false;
			}

			if (expected == null && actual != null)
			{
				return false;
			}

			if (expected == null && actual == null)
			{
				return true;
			}

			foreach (System.Reflection.PropertyInfo propertyInfo in typeof(T).GetProperties())
			{
				// these tries are here because when you access fields that have not been populated, we except... -- biedrzycki May 20th, 2016
				int exceptionCount = 0;
				object expectedValue = null;
				object actualValue = null;

				try
				{
					expectedValue = propertyInfo.GetValue(expected);
				}
				catch (Exception)
				{
					exceptionCount++;
				}

				try
				{
					actualValue = propertyInfo.GetValue(actual);
				}
				catch (Exception)
				{
					exceptionCount++;
				}

				if (exceptionCount == 2)
				{
					continue; // both properties have not been set on the model object
				}

				if (exceptionCount == 1)
				{
					return false; // one object has a property set that the other does not
				}

				if (expectedValue != null && actualValue == null)
				{
					return false;
				}

				if (expectedValue == null && actualValue != null)
				{
					return false;
				}

				if (expectedValue == null && actualValue == null)
				{
					continue;
				}

				bool propertyIsEnumerable = expectedValue is IEnumerable;
				bool propertyIsClass = expectedValue.GetType().IsClass;
				if (propertyIsEnumerable)
				{
					IEnumerable<object> expectedEnumerable = expectedValue as IEnumerable<object>;
					IEnumerable<object> actualEnumerable = actualValue as IEnumerable<object>;

					if (expectedEnumerable.Count() != actualEnumerable.Count())
					{
						return false;
					}

					for (int i = 0; i < expectedEnumerable.Count(); i++)
					{
						bool itemsMatch = Matches(expectedEnumerable.ElementAt(i), actualEnumerable.ElementAt(i));
						if (!itemsMatch)
						{
							return false;
						}
					}
				}
				else if (propertyIsClass)
				{
					bool classesMatch = Matches(expectedValue, actualValue);
					if (!classesMatch)
					{
						return false;
					}
				}
				else if (!expectedValue.Equals(actualValue))
				{
					return false;
				}
			}

			return true;
		}

		#region SQL Helpers
		public static Boolean MatchesSqlParameters(IEnumerable<SqlParameter> expectedSqlParameters,
			IEnumerable<SqlParameter> actualSqlParameters)
		{
			if (expectedSqlParameters == null && actualSqlParameters == null)
			{
				return true;
			}

			if (expectedSqlParameters != null && actualSqlParameters == null)
			{
				throw new Exception("Expected non-null parameters. Parameters were null");
			}

			if (expectedSqlParameters == null && actualSqlParameters != null)
			{
				throw new Exception("Expected null parameters. Parameters were non-null");
			}

			List<SqlParameter> expected = expectedSqlParameters.ToList();
			List<SqlParameter> actual = actualSqlParameters.ToList();

			if (expected.Count != actual.Count)
			{
				throw new Exception(string.Format("Expected {0} parameters. Got {1} parameters", expected.Count, actual.Count));
			}

			for (int i = 0; i < expected.Count; i++)
			{
				if (!MatchesSqlParameter(expected[i], actual[i]))
				{
					throw new Exception(string.Format("Expected parameter {0} value of {1}. Actual value {2}", expected[i].ParameterName, expected[i].Value, actual[i].Value));
				}
			}

			return true;
		}

		/// <summary>
		/// Validates that the individual sql parameter matches the expected value
		/// </summary>
		/// <param name="expected">Expected sql parameter</param>
		/// <param name="actual">Actual sql parameter</param>
		/// <returns>True if sql parameters match, otherwise false</returns>
		private static Boolean MatchesSqlParameter(SqlParameter expected, SqlParameter actual)
		{
			if (expected != null && actual == null)
			{
				return false;
			}

			if (expected == null && actual != null)
			{
				return false;
			}

			if (expected.SqlDbType != actual.SqlDbType)
			{
				return false;
			}

			if (expected.ParameterName != actual.ParameterName)
			{
				return false;
			}

			if (expected.Value != null)
			{
				try
				{
					if ((string)expected.Value != (string)actual.Value)
					{
						return false;
					}
				}
				catch (InvalidCastException)
				{
					try
					{
						if ((int)expected.Value != (int)actual.Value)
						{
							return false;
						}
					}
					catch (InvalidCastException)
					{
					}
				}
			}

			return true;
		}
		#endregion
	}
}
