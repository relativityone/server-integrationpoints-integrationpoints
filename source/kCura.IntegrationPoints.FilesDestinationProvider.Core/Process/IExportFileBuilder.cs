﻿using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public interface IExportFileBuilder
    {
        ExportFile Create(ExportSettings exportSettings);
    }
}