using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class EnumerableReader : TextReader
    {
        private readonly char _separator;
        private readonly IEnumerator<string> _enumerator;
        private string _currentText;
        private int _pos;
        private readonly bool _useSeperator;
        private bool _enumeratorDisposed = false;

        public EnumerableReader(IEnumerable<string> lines) : this(lines, '\n')
        {
            _useSeperator = false;
        }

        public EnumerableReader(IEnumerable<string> lines, char separator)
        {
            _enumerator = lines.GetEnumerator();
            _separator = separator;
            _useSeperator = true;
        }

        private void SetNewLine()
        {
            _currentText = _enumerator.MoveNext() ? _enumerator.Current : null;
            _pos = 0;
        }

        public override int Read()
        {
            CheckReaderNotClosed();

            if (_currentText == null)
            {
                SetNewLine();
                if (_currentText == null)
                    return -1;
            }

            if (_pos >= _currentText.Length)
            {
                SetNewLine();
                if (_currentText == null)
                    return -1;

                if (_useSeperator)
                    return _separator;
            }

            return _currentText[_pos++];
        }

        public override int Peek()
        {
            CheckReaderNotClosed();

            if (_currentText == null)
            {
                SetNewLine();
                if (_currentText == null)
                    return -1;
            }

            string newst = _currentText;
            int newpos = _pos;

            if (_pos >= _currentText.Length)
            {
                if (_useSeperator)
                    return _separator;

                SetNewLine();
                if (_currentText == null)
                    return -1;

                newst = _currentText;
                newpos = _pos;
            }

            return newst[newpos];
        }

        private void CheckReaderNotClosed()
        {
            if (_enumeratorDisposed)
                throw new ObjectDisposedException(null, "The reader has already been closed");
        }

        public override int Read(char[] buffer, int index, int count)
        {
            for (int j = 0; j < index; j++)
            {
                //Consume chars
                Read();
            }

            int i = 0;
            while (i < count)
            {
                int val = Read();
                if (val == -1)
                {
                    break;
                }
                buffer[i] = (char) val;
                i++;
            }
            return i;
        }

        public override string ReadToEnd()
        {
            var sb = new StringBuilder();

            string line;
            bool first = true;
            while ((line = ReadLine()) != null)
            {
                sb.Append((!first && _useSeperator) ? _separator + line : line);
                first = false;
            }

            return sb.ToString();
        }

        public override string ReadLine()
        {
            CheckReaderNotClosed();
            SetNewLine();
            return _currentText;
        }

        protected override void Dispose(bool disposing)
        {
            _enumerator.Dispose();
            _enumeratorDisposed = true;
            base.Dispose(disposing);
        }
    }
}