using System.ComponentModel;
using System.Reflection;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void InitializePropertyDefaultValues(this object obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DefaultValueAttribute d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                {
                    prop.SetValue(obj, d.Value);
                }
            }
        }
    }
}
