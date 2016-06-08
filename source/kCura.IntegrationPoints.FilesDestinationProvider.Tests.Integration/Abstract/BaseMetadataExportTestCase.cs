using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
    public abstract class BaseMetadataExportTestCase : BaseExportTestCase
    {
        public abstract string MetadataFormat { get; }

        public string FileFormat => $"*.{MetadataFormat}";

        protected FileInfo GetFileInfo(DirectoryInfo directory)
        {
            return DataFileFormatHelper.GetFileInFormat(FileFormat, directory);
        }
    }
}
