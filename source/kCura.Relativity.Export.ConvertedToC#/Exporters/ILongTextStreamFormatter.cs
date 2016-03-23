using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;

using kCura.Relativity.Export.FileObjects;

namespace kCura.Relativity.Export.Exports
{
    public interface ILongTextStreamFormatter
    {
        void TransformAndWriteCharacter(Int32 character, System.IO.TextWriter outputStream);
    }
    public class NonTransformFormatter : ILongTextStreamFormatter
    {
        public void TransformAndWriteCharacter(int character, System.IO.TextWriter outputStream)
        {
            outputStream.Write(Strings.ChrW(character));
        }
    }
    public class HtmlFileLongTextStreamFormatter : ILongTextStreamFormatter
    {


        private System.IO.TextReader _source;
        public HtmlFileLongTextStreamFormatter(ExportFile settings, System.IO.TextReader source)
        {
            _source = source;
        }

        public void TransformAndWriteCharacter(int character, System.IO.TextWriter outputStream)
        {
            switch (character)
            {
                case 13:
                    outputStream.Write("<br/>");
                    if (_source.Peek() == 10)
                        _source.Read();
                    break;
                case 10:
                    outputStream.Write("<br/>");
                    break;
                default:
                    outputStream.Write(System.Web.HttpUtility.HtmlEncode(Strings.ChrW(character)));
                    break;
            }
        }

    }

    public class DelimitedFileLongTextStreamFormatter : ILongTextStreamFormatter
    {
        private char _quoteDelimiter;
        private char _newlineDelimiter;

        private System.IO.TextReader _source;
        public DelimitedFileLongTextStreamFormatter(ExportFile settings, System.IO.TextReader source)
        {
            _quoteDelimiter = settings.QuoteDelimiter;
            _newlineDelimiter = settings.NewlineDelimiter;
            _source = source;
        }

        public void TransformAndWriteCharacter(Int32 character, System.IO.TextWriter outputStream)
        {

            if (character == Strings.AscW(_quoteDelimiter))
            {
                outputStream.Write(_quoteDelimiter + _quoteDelimiter);
            }
            else if (character == 13 || character == 10)
            {
                outputStream.Write(_newlineDelimiter);
                if (_source.Peek() == 10)
                {
                    _source.Read();
                }
            }
            else {
                outputStream.Write(Strings.ChrW(character));
            }
        }

    }


}

