using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class EnumerableReader : TextReader
    {
        private readonly IEnumerator<string> _enumerator;
        private string _currentText;
        private int _pos;
        private bool _enumeratorDisposed = false;

        public EnumerableReader(IEnumerable<string> lines)
        {
            _enumerator = lines.GetEnumerator();
        }

        private void SetNewLine()
        {
            _currentText = _enumerator.MoveNext() ? _enumerator.Current : null;
            if (_currentText != null && !_currentText.EndsWith(Environment.NewLine))
            {
                _currentText = _currentText + Environment.NewLine;
            }
            _pos = 0;
        }

        public override int Read()
        {
            CheckReaderNotClosed();

            if (_currentText == null)
            {
                SetNewLine();
                if (_currentText == null)
                {
                    return -1;
                }
            }

            if (_pos >= _currentText.Length)
            {
                SetNewLine();
                if (_currentText == null)
                {
                    return -1;
                }
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
                {
                    return -1;
                }
            }

            string newst = _currentText;
            int newpos = _pos;

            if (_pos >= _currentText.Length)
            {
                SetNewLine();
                if (_currentText == null)
                {
                    return -1;
                }
                newst = _currentText;
                newpos = _pos;
            }

            return newst[newpos];
        }

        private void CheckReaderNotClosed()
        {
            if (_enumeratorDisposed)
            {
                throw new ObjectDisposedException(null, "The reader has already been closed");
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int i = 0;
            while (i < count)
            {
                int val = Read();
                if (val == -1)
                {
                    break;
                }
                buffer[index + i] = (char)val;
                i++;
            }
            return i;
        }

        public override string ReadToEnd()
        {
            var sb = new StringBuilder();

            string line;
            while ((line = ReadLine()) != null)
            {
                sb.Append(line);
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