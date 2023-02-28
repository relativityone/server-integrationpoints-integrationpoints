using System.Collections;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation.TestCases
{
    public class CollectionFieldValueTestCase
    {
        public ICollection Value { get; }

        public string Name { get; }

        public CollectionFieldValueTestCase(ICollection value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
