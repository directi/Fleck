using System;

namespace Tasks
{
    public class CancellationTokenSource
    {
        private bool _cancelled;

        public bool Cancelled
        {
            get { return _cancelled; }
        }

        public event EventHandler IsCancelled;

        public CancellationToken Token
        {
            get { return new CancellationToken(this); }
        }

        public void Cancel()
        {
            _cancelled = true;
            if (IsCancelled != null) IsCancelled(this, null);
        }
    }
}
