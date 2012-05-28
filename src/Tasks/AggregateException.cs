using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks
{
    public class AggregateException : Exception
    {
        private IEnumerable<Exception> _exceptions;

        public AggregateException()
        {
            _exceptions = new List<Exception>();
        }

        public AggregateException(IEnumerable<Exception> exceptions)
        {
            _exceptions = exceptions;
        }

        public override string ToString()
        {
            return String.Join("/n/n", _exceptions.Select((exception, i) => exception.ToString()).ToArray());
        }
    }
}
