using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    internal static class FmsConstants
    {
        public const string ApiV1UrlPart = @"api/v1/DataFactory";
        public const string CopyListOfFilesMethodName = "CopyListOfFiles";
        public const string GetStatusMethodName = "GetStatus";
        public const string CancelMethodName = "Cancel";
    }
}
