using System;
using System.Collections;
using System.Collections.Generic;
using Tasks;

namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}

public static class ListExtensions
{
    public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, int count)
    {
        IEnumerator<T> enumerator = new SkipEnumerator<T>(source.GetEnumerator(), count);
        return new WrapperEnumerable<T>(enumerator);
    }

    public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int count)
    {
        IEnumerator<T> enumerator = new TakeEnumerator<T>(source.GetEnumerator(), count);
        return new WrapperEnumerable<T>(enumerator);
    }

    public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        IEnumerator<T> enumerator = new WhereEnumerator<T>(source.GetEnumerator(), predicate);
        return new WrapperEnumerable<T>(enumerator);
    }

    public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> selector)
    {
        IEnumerator<TResult> enumerator = new SelectEnumerator<T, TResult>(source.GetEnumerator(), selector);
        return new WrapperEnumerable<TResult>(enumerator);
    } 

    public static T[] ToArray<T>(this IEnumerable<T> source)
    {
        return new List<T>(source).ToArray();
    }

    public static List<T> ToList<T>(this IEnumerable<T> source)
    {
        return new List<T>(source);
    }

    public static int Count<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var count = 0;
        IEnumerator<T> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (predicate(enumerator.Current)) count++;
        }
        return count;
    }

    public static bool Contains<T>(this IEnumerable<T> source, T item)
    {
        IEnumerator<T> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (EqualityComparer<T>.Default.Equals(enumerator.Current, item)) return true;
        }
        return false;
    }


    private abstract class DelegatingEnumerator<T> : IEnumerator<T>
    {
        protected IEnumerator<T> _enumerator;

        protected DelegatingEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public virtual void Dispose()
        {
            _enumerator.Dispose();
        }

        public virtual bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException("Specified method is not supported");
        }

        public T Current
        {
            get { return _enumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }




    private class TakeEnumerator<T> : DelegatingEnumerator<T>
    {
        private int _count;

        public TakeEnumerator(IEnumerator<T> enumerator, int count)
            : base(enumerator)
        {
            _count = count;
        }

        public override bool MoveNext()
        {
            if (_count > 0)
            {
                bool moveNext = _enumerator.MoveNext();
                if (moveNext)
                {
                    _count--;
                }
                return moveNext;
            }
            return false;
        }
    }

    private class SkipEnumerator<T> : DelegatingEnumerator<T>
    {
        public SkipEnumerator(IEnumerator<T> enumerator, int count)
            : base(enumerator)
        {
            for (int i = 0; i < count; i++)
            {
                enumerator.MoveNext();
            }
        }
    }

    private class WhereEnumerator<T> : DelegatingEnumerator<T>
    {
        private readonly Func<T, bool> _predicate;

        public WhereEnumerator(IEnumerator<T> enumerator, Func<T, bool> predicate)
            : base(enumerator)
        {
            _predicate = predicate;
        }

        public override bool MoveNext()
        {
            bool hasNext = _enumerator.MoveNext();
            while (hasNext && !_predicate(_enumerator.Current))
            {
                hasNext = _enumerator.MoveNext();
            }
            return hasNext;
        }
    }

    private class SelectEnumerator<T, TResult> : DelegatingEnumerator<T>, IEnumerator<TResult>
    {
        private readonly Func<T, Int32, TResult> _selector;
        private Int32 _index;

        public SelectEnumerator(IEnumerator<T> enumerator, Func<T, Int32, TResult> selector)
            : base(enumerator)
        {
            _selector = selector;
            _index = -1;
        }

        public new TResult Current
        {
            get { return _selector(_enumerator.Current, _index); }
        }

        public override bool MoveNext()
        {
            bool next = base.MoveNext();
            if (next) _index++;
            return next;
        }
    }

    private class WrapperEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public WrapperEnumerable(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerator;
        }
    }
}