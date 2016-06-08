using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public class Exceptions
    {
        public class DuplicateColumnsExistExcepetion : Exception
        {
            public DuplicateColumnsExistExcepetion()
                : base("Duplicate columns exist in file, please fix and reload.")
            { }
            public DuplicateColumnsExistExcepetion(string message)
                : base(message)
            { }
            public DuplicateColumnsExistExcepetion(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class BlankColumnExcepetion : Exception
        {
            public BlankColumnExcepetion()
                : base("There is a blank column in your header row, please enter a name for the column and reload.")
            { }
            public BlankColumnExcepetion(string message)
                : base(message)
            { }
            public BlankColumnExcepetion(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class NumberOfColumnsNotEqualToNumberOfDataValuesException : Exception
        {
            public NumberOfColumnsNotEqualToNumberOfDataValuesException(Int32 lineNumber)
                : base(String.Format("The are a different number of data columns than column heading in line number {0}.", lineNumber))
            { }
            public NumberOfColumnsNotEqualToNumberOfDataValuesException(string message)
                : base(message)
            { }
            public NumberOfColumnsNotEqualToNumberOfDataValuesException(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class NoColumnsExcepetion : Exception
        {
            public NoColumnsExcepetion()
                : base("No columns are present in load file.")
            { }
            public NoColumnsExcepetion(string message)
                : base(message)
            { }
            public NoColumnsExcepetion(string message, Exception inner)
                : base(message, inner)
            { }
        }

        public class ColumnsMissmatchExcepetion : Exception
        {
            public ColumnsMissmatchExcepetion()
                : base("File contains different set or order of columns then original format.")
            { }
            public ColumnsMissmatchExcepetion(string message)
                : base(message)
            { }
            public ColumnsMissmatchExcepetion(string message, Exception inner)
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
