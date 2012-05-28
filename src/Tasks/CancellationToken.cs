using System;

namespace Tasks
{
    public class CancellationToken
    {
        private bool _cancelled;

        public CancellationToken(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.IsCancelled += Cancel;
            _cancelled = false;
        }

        public bool IsCancelled
        {
            get {
                return _cancelled;
            }
        }

        private void Cancel(object sender, EventArgs e)
        {
            _cancelled = true;
        }

        public void ThrowIfCancellationRequested()
        {
            if (_cancelled) throw new OperationCanceledException();
        }
    }
}
