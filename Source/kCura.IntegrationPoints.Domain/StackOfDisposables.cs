using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain
{
    /// <summary>
    /// This class will dispose pushed IDisposable objects in LIFO manner
    /// </summary>
    public class StackOfDisposables : IDisposable
    {
        private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

        public void Push(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StackOfDisposables()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (_disposables.Count > 0)
                {
                    IDisposable disposable = _disposables.Pop();
                    disposable?.Dispose();
                }
            }
        }
    }
}
