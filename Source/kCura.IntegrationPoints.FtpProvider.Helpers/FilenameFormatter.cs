using System;
using System.Linq;
using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

[assembly: InternalsVisibleTo("kCura.IntegrationPoints.FtpProvider.Helpers.Tests")]

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public static class FilenameFormatter
    {
        public static String FormatFilename(String filename, Char wildCard, DateTime date)
        {
            var indexes = FindAllIndexes(filename, wildCard);
            var replacements = FindTextToBeReplaced(filename, indexes, date);
            var final = InsertReplacementText(filename, replacements, wildCard);
            return final;
        }
        internal static Int32[] FindAllIndexes(String filename, Char wildCard)
        {
            var cnt = filename.Count(c => c == wildCard);
            if (cnt % 2 != 0)
            {
                throw new Exceptions.UnevenNumberOfWildCardCharactersException();
            }
            var retVal = new Int32[cnt];
            var retValCnt = 0;

            for (var i = 0; i < filename.Length; i++)
            {
                if (filename[i].Equals(wildCard))
                {
                    retVal[retValCnt] = i;
                    retValCnt++;
                }
            }
            return retVal;
        }

        internal static TextReplacement[] FindTextToBeReplaced(String input, Int32[] indexes, DateTime date)
        {
            var retVal = new TextReplacement[indexes.Length / 2];
            var replacementCnt = 0;
            for (var i = 0; i < indexes.Length; i += 2)
            {
                var start = indexes[i];
                var end = indexes[i + 1];
                var original = input.Substring(start, end - start + 1);

                retVal[replacementCnt] = new TextReplacement
                {
                    StartIndex = start,
                    EndIndex = end,
                    OriginalText = original,
                    UpdatedText = date.ToString(original.Substring(1, original.Length - 2))
                };
                replacementCnt++;
            }
            return retVal;
        }

        internal static String InsertReplacementText(String filename, TextReplacement[] replacements, Char searchChar)
        {
            var updatedText = filename;

            for (var i = 0; i < replacements.Count(); i++)
            {
                var rep = replacements[i];
                var beginningPart = updatedText.Substring(0, updatedText.IndexOf(searchChar, 0));
                var endPart = updatedText.Substring(updatedText.IndexOf(searchChar, beginningPart.Length + 1) + 1);
                updatedText = beginningPart + rep.UpdatedText + endPart;
            }
            return updatedText;
        }

        public static String FormatFtpPath(String path)
        {
            var retVal = path.Trim();
            retVal = retVal.Replace("\\", "/");
            if (retVal[0] != '/')
            {
                retVal = "/" + retVal;
            }
            return retVal;
        }
    }
}
