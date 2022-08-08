using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core
{
    internal static class EnumHelper
    {
        internal static void Parse<T>(string value, out T output) where T : struct
        {
            T? parsedOutput;
            if (!TryParse(value, out parsedOutput))
            {
                throw new InvalidEnumArgumentException($"Unknown {typeof(T).Name} ({value})");
            }
            output = parsedOutput.Value;
        }
        internal static bool TryParse<T>(string value, out T? output) where T : struct
        {
            T parsedOutput;
            if (Enum.TryParse(value, true, out parsedOutput))
            {
                if (Enum.IsDefined(typeof(T), parsedOutput))
                {
                    output = parsedOutput;
                    return true;
                }
            }
            output = null;
            return false;
        }
    }
}
