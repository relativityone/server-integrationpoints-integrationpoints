using System.IO;
using System.Text;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Microsoft.VisualBasic.FileIO;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    internal class TextFieldParserWrapper : IFieldParser
    {
        private readonly TextFieldParser _textFieldParser;

        public bool EndOfData => _textFieldParser.EndOfData;

        public long LineNumber => _textFieldParser.LineNumber;

        public string ErrorLine => _textFieldParser.ErrorLine;

        public long ErrorLineNumber => _textFieldParser.ErrorLineNumber;

        public string[] CommentTokens
        {
            get => _textFieldParser.CommentTokens;

            set => _textFieldParser.CommentTokens = value;
        }

        public FieldType TextFieldType
        {
            get => _textFieldParser.TextFieldType;

            set => _textFieldParser.TextFieldType = value;
        }

        public int[] FieldWidths
        {
            get => _textFieldParser.FieldWidths;

            set => _textFieldParser.FieldWidths = value;
        }

        public string[] Delimiters
        {
            get => _textFieldParser.Delimiters;

            set => _textFieldParser.Delimiters = value;
        }

        public bool TrimWhiteSpace
        {
            get => _textFieldParser.TrimWhiteSpace;

            set => _textFieldParser.TrimWhiteSpace = value;
        }

        public bool HasFieldsEnclosedInQuotes
        {
            get => _textFieldParser.HasFieldsEnclosedInQuotes;

            set => _textFieldParser.HasFieldsEnclosedInQuotes = value;
        }

        public TextFieldParserWrapper(string path)
        {
            _textFieldParser = new TextFieldParser(path);
        }

        public TextFieldParserWrapper(string path, Encoding defaultEncoding)
        {
            _textFieldParser = new TextFieldParser(path, defaultEncoding);
        }

        public TextFieldParserWrapper(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            _textFieldParser = new TextFieldParser(path, defaultEncoding, detectEncoding);
        }

        public TextFieldParserWrapper(Stream stream)
        {
            _textFieldParser = new TextFieldParser(stream);
        }

        public TextFieldParserWrapper(Stream stream, Encoding defaultEncoding)
        {
            _textFieldParser = new TextFieldParser(stream, defaultEncoding);
        }

        public TextFieldParserWrapper(Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            _textFieldParser = new TextFieldParser(stream, defaultEncoding, detectEncoding);
        }

        public TextFieldParserWrapper(
            Stream stream,
            Encoding defaultEncoding,
            bool detectEncoding,
            bool leaveOpen)
        {
            _textFieldParser = new TextFieldParser(stream, defaultEncoding, detectEncoding, leaveOpen);
        }

        public TextFieldParserWrapper(TextReader reader)
        {
            _textFieldParser = new TextFieldParser(reader);
        }

        public string[] ReadFields()
        {
            return _textFieldParser.ReadFields();
        }

        public string ReadLine()
        {
            return _textFieldParser.ReadLine();
        }

        public string ReadToEnd()
        {
            return _textFieldParser.ReadToEnd();
        }

        public string PeekChars(int numberOfChars)
        {
            return _textFieldParser.PeekChars(numberOfChars);
        }

        public void SetDelimiters(params string[] delimiters)
        {
            _textFieldParser.SetDelimiters(delimiters);
        }

        public void SetFieldWidths(params int[] fieldWidths)
        {
            _textFieldParser.SetFieldWidths(fieldWidths);
        }

        public void Close()
        {
            _textFieldParser.Close();
        }

        public void Dispose()
        {
            _textFieldParser.Dispose();
        }
    }
}