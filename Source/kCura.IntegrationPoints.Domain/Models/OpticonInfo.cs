using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public static class OpticonInfo
    {
        public const string BATES_NUMBER_FIELD_NAME = "BatesNumber";
        public const string DOCUMENT_ID_FIELD_NAME = "DocumentIdentifier";
        public const string FILE_LOCATION_FIELD_NAME = "FileLocation";
        public const char OPTICON_RECORD_DELIMITER = ',';

        public const int BATES_NUMBER_FIELD_INDEX = 0;
        public const int FILE_LOCATION_FIELD_INDEX = 1;
        public const int DOCUMENT_ID_FIELD_INDEX = 2;
    }
}
