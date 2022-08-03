using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.Documents
{
    public class FileEncodingModel
    {
        /// <summary>
        /// Creates model with default values for Load file.
        /// </summary>
        /// <param name="encoding">Encoding of Load file, with UTF-8 default value.</param>
        /// <returns>New instance of FileEncodingModel</returns>
        public static FileEncodingModel CreateDefault(string encoding = LoadFileEncodingConstants.UTF_8)
        {
            var model = new FileEncodingModel
            {
                FileEncoding = encoding,
                Column = 20,
                Quote = 254,
                Newline = 174,
                MultiValue = 59,
                NestedValue = 92
            };

            return model;
        }

        public string FileEncoding { get; set; }

        public int Column { get; set; }

        public int Quote { get; set; }

        public int Newline { get; set; }

        public int MultiValue { get; set; }

        public int NestedValue { get; set; }
    }
}
