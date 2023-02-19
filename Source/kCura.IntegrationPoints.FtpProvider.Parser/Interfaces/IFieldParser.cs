using System;
using Microsoft.VisualBasic.FileIO;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IFieldParser : IDisposable
    {
        bool EndOfData { get; }

        long LineNumber { get; }

        string ErrorLine { get; }

        long ErrorLineNumber { get; }

        string[] CommentTokens { get; set; }

        FieldType TextFieldType { get; set; }

        int[] FieldWidths { get; set; }

        string[] Delimiters { get; set; }

        bool TrimWhiteSpace { get; set; }

        bool HasFieldsEnclosedInQuotes { get; set; }

        string[] ReadFields();

        string ReadLine();

        string ReadToEnd();

        string PeekChars(int numberOfChars);

        void SetDelimiters(params string[] delimiters);

        void SetFieldWidths(params int[] fieldWidths);

        void Close();
    }
}
