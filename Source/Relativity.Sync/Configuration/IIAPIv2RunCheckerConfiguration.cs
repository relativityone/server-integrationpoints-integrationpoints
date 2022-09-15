using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
    internal interface IIAPIv2RunCheckerConfiguration
    {
        ImportNativeFileCopyMode NativeBehavior { get; }

        bool ImageImport { get; }

        int RdoArtifactTypeId { get; }

        bool IsRetried { get; }

        bool IsDrainStopped { get; }

        bool HasLongTextFields { get; }

    }
}
