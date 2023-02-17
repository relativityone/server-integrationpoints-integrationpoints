using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public class Exceptions
    {
        public class DuplicateColumnsExistException : Exception
        {
            public DuplicateColumnsExistException()
                : base("Duplicate columns exist in file, please fix and reload.")
            { }

            public DuplicateColumnsExistException(string message)
                : base(message)
            { }

            public DuplicateColumnsExistException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class BlankColumnException : Exception
        {
            public BlankColumnException()
                : base("There is a blank column in your header row, please enter a name for the column and reload.")
            { }

            public BlankColumnException(string message)
                : base(message)
            { }

            public BlankColumnException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class NumberOfColumnsNotEqualToNumberOfDataValuesException : Exception
        {
            public NumberOfColumnsNotEqualToNumberOfDataValuesException(long lineNumber)
                : base($"The are a different number of data columns than column heading in line number {lineNumber}.")
            { }

            public NumberOfColumnsNotEqualToNumberOfDataValuesException(string message)
                : base(message)
            { }

            public NumberOfColumnsNotEqualToNumberOfDataValuesException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class NoColumnsException : Exception
        {
            public NoColumnsException()
                : base("No columns are present in load file.")
            { }

            public NoColumnsException(string message)
                : base(message)
            { }

            public NoColumnsException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class ColumnsMissmatchException : Exception
        {
            public ColumnsMissmatchException()
                : base("File contains different set or order of columns then original format.")
            { }

            public ColumnsMissmatchException(string message)
                : base(message)
            { }

            public ColumnsMissmatchException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class CantAccessSourceException : Exception
        {
            public CantAccessSourceException()
                : base("Source does not exist or you do not have access.")
            { }

            public CantAccessSourceException(string message)
                : base(message)
            { }

            public CantAccessSourceException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class UnevenNumberOfWildCardCharactersException : Exception
        {
            public UnevenNumberOfWildCardCharactersException()
                : base("Please re-enter the filename, there are an uneven number of wildcard characters")
            { }

            public UnevenNumberOfWildCardCharactersException(string message)
                : base(message)
            { }

            public UnevenNumberOfWildCardCharactersException(string message, Exception inner)
                : base(message, inner)
            { }
        }

    }
}
