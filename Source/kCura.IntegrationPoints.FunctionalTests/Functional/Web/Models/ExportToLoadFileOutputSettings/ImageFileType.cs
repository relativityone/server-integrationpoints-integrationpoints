using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    
    public enum ImageFileTypes
    {
        [Term("Single page TIFF/JPEG")]
        SinglePage,
        [Term("Multi page TIFF/JPEG")]
        MultiPage,
        PDF
    }

    public enum ImagePrecedences
    {
        OriginalImages,
        ProducedImages
    }
}
