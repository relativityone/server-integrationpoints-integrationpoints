using System.Collections;
using System.Collections.Generic;

//Guidelines for class from: https://msdn.microsoft.com/en-us/library/9eekhta0(v=vs.110).aspx

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class EnumerableParser : IEnumerable<string[]>
    {
        private IEnumerable<string> _sourceFileLines;
        private char _columnDelimiter;

        public EnumerableParser(IEnumerable<string> sourceFileLines, char columnDelimiter)
        {
            _sourceFileLines = sourceFileLines;
            _columnDelimiter = columnDelimiter;
        }

        public IEnumerator<string[]> GetEnumerator()
        {
            return new EnumerableParserEnumerator(_sourceFileLines, _columnDelimiter);
        }

        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
    }

    public class EnumerableParserEnumerator : IEnumerator<string[]>
    {
        private IEnumerable<string> _sourceFileLines;
        private IEnumerator<string> _sourceFileEnumerator;
        private string[] _columnDelimiter;
        private string[] _current;
        private bool _hasNext;

        public EnumerableParserEnumerator(IEnumerable<string> sourceFileLines, char columnDelimiter)
        {
            _columnDelimiter = new string[] { columnDelimiter.ToString() };
            _sourceFileLines = sourceFileLines;
            _hasNext = true;
            ResetEnumerator();
        }

        ~EnumerableParserEnumerator()
        {
            this.Dispose();
        }

        public string[] Current
        {
            get { return _current; }
        }

        private object Current1
        {
            get { return this.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current1; }
        }

        public bool MoveNext()
        {
            bool rv = _hasNext;
            if (rv)
            {
                _current = _sourceFileEnumerator.Current.Split(_columnDelimiter, System.StringSplitOptions.RemoveEmptyEntries);
                _hasNext = _sourceFileEnumerator.MoveNext();
            }
            return rv;
        }

        public void Reset()
        {
            ResetEnumerator();
        }

        public void Dispose()
        {
            _sourceFileEnumerator.Dispose();
        }

        private void ResetEnumerator()
        {
            _sourceFileEnumerator = _sourceFileLines.GetEnumerator();
            _hasNext = _sourceFileEnumerator.MoveNext();
        }
    }
}