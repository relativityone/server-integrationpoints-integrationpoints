using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    // TODO: This class should be moved into a shared Test project -- biedrzycki: May 19th, 2016
    public static class MatchHelper
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
                object expectedValue = null;
                object actualValue = null;

                expectedValue = propertyInfo.GetValue(expected);
                actualValue = propertyInfo.GetValue(actual);

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

                bool propertyIsString = expectedValue is string;
                bool propertyIsEnumerable = expectedValue is IEnumerable;
                bool propertyIsClass = expectedValue.GetType().IsClass;
                if (propertyIsString)
                {
                    string expectedString = expectedValue as string;
                    string actualString = actualValue as string;
                    if (!String.Equals(expectedString, actualString, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
                else if (propertyIsEnumerable)
                {
                    IEnumerable<object> expectedEnumerable = expectedValue as IEnumerable<object>;
                    IEnumerable<object> actualEnumerable = actualValue as IEnumerable<object>;

                    if (expectedEnumerable?.Count() != actualEnumerable?.Count())
                    {
                        return false;
                    }

                    for (int i = 0; i < expectedEnumerable?.Count(); i++)
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
    }
}