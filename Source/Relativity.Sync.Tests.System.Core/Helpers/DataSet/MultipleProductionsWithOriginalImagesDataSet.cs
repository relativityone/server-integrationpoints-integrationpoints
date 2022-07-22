using System;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers.DataSet
{
    /// <summary>
    /// DataSet Documents
    /// <para>DOC_1 - Original</para>
    /// <para>DOC_2 - Original</para>
    /// <para>DOC_3 - Production1</para>
    /// <para>DOC_4 - Production1, Production2</para>
    /// <para>DOC_5 - Production2</para>
    /// <para>DOC-6 - Without Images</para>
    /// <para>DOC-7 - Without Images</para>
    /// </summary>
    public class MultipleProductionsWithOriginalImagesDataSet
    {
        private const string _BASE_PATH = "MultipleProductionsWithOriginalImages";

        public Dataset DocumentsWithFirstProductionDataSet { get; }

        public Dataset DocumentsWithSecondProductionDataSet { get; }

        public Dataset DocumentsWithOriginalImagesDataSet { get; }

        public Dataset DocumentsWithoutImagesDataSet { get; }

        private MultipleProductionsWithOriginalImagesDataSet()
        {
            string ProductionControlNumberFunc(FileInfo fileInfo)
            {
                return fileInfo.Name.Split('-').First();
            }

            DocumentsWithFirstProductionDataSet = CreateProductionDataSetWithBasePath("Production1", ImportType.Production, ProductionControlNumberFunc);

            DocumentsWithSecondProductionDataSet = CreateProductionDataSetWithBasePath("Production2", ImportType.Production, ProductionControlNumberFunc);
            
            DocumentsWithOriginalImagesDataSet = CreateDataSetWithBasePath("OriginalImages", ImportType.Image);

            DocumentsWithoutImagesDataSet = CreateDataSetWithBasePath("WithoutImages", ImportType.Native);
        }

        public static MultipleProductionsWithOriginalImagesDataSet Create()
        {
            return new MultipleProductionsWithOriginalImagesDataSet();
        }

        private Dataset CreateDataSetWithBasePath(string name, ImportType importType)
        {
            string setPath = Path.Combine(_BASE_PATH, name);

            return new Dataset(setPath, importType);
        }

        private Dataset CreateProductionDataSetWithBasePath(string name, ImportType importType, Func<FileInfo, string> controlNumberFunc = null)
        {
            string setPath = Path.Combine(_BASE_PATH, name);

            return new Dataset(setPath, importType, begBatesGetter: controlNumberFunc);
        }

    }
}
