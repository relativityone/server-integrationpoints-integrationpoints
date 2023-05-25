using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public enum TaggingOptionEnum
    {
        /// <summary>
        /// Documents will be tagged both in source and destination workspace.
        /// </summary>
        Enabled,

        /// <summary>
        /// Documents will be tagged only in destination workspace.
        /// </summary>
        DestinationOnly,

        /// <summary>
        /// Documents will not be tagged.
        /// </summary>
        Disabled
    }
}
