using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;

namespace Tasks
{
    [HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true,
    ExternalThreading = true)]
    public class Task
    {
        protected readonly TaskFactory _factory;
        private bool _started;
        private bool _completed;
        private bool _faulted;
        private Action _action;
        private ManualResetEvent _mre;
        private AggregateException _exception;
        private readonly List<Task> _completedContinuations;
        private readonly List<Task> _faultedContinuations;
        private readonly List<Task> _continuations;
        private bool _cancelled;

        internal Task(TaskFactory factory)
        {
            _factory = factory;
            _started = false;
            _completed = false;
            _faulted = false;
            _cancelled = false;
            _mre = new ManualResetEvent(false);
            _completedContinuations = new List<Task>();
            _faultedContinuations = new List<Task>();
            _continuations = new List<Task>();
        } 


        public Task(Action action, TaskFactory factory) :this(factory)
        {
            _action = action;
        }

        internal void Start()
        {
            if (_factory.CancellationToken != null && _factory.CancellationToken.IsCancelled)
            {
                _cancelled = true;
                return;
            }
            if (_started) throw new InvalidOperationException("Task has already been started");
            _started = true;
            DoStart();
        }

        private void DoStart()
        {
            ThreadPool.QueueUserWorkItem(_ =>
                                             {
                                                 try
                                                 {
                                                     RunActualTask();
                                                     _completed = true;
                                                     _completedContinuations.ForEach(task => task.Start());
                                                 }
                                                 catch (Exception ex)
                                                 {
                                                     _exception = new AggregateException(new[] {ex});
                                                     _faulted = true;
                                                     _faultedContinuations.ForEach(task => task.Start());
                                                 }
                                                 _mre.Set();
                                                 _continuations.ForEach(task => task.Start());
                                             });
        }

        protected virtual void RunActualTask()
        {
            _action();
        }

        public bool IsFaulted
        {
            get { return _faulted; }
        }

        public bool IsCompleted
        {
            get { return _completed; }
        }

        public Task ContinueWith(Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
        {
            return HandleContinuationTask(_factory.ContinuationTask(this, continuationAction), continuationOptions);
        }

        protected Task HandleContinuationTask(Task continuationTask, TaskContinuationOptions continuationOptions)
        {
            if (((continuationOptions & TaskContinuationOptions.OnlyOnRanToCompletion) == TaskContinuationOptions.OnlyOnRanToCompletion) ||
                ((continuationOptions & TaskContinuationOptions.NotOnFaulted) == TaskContinuationOptions.NotOnFaulted))
            {
                _completedContinuations.Add(continuationTask);
                if (_completed) continuationTask.Start();
            }
            else if ((continuationOptions & TaskContinuationOptions.OnlyOnFaulted) == TaskContinuationOptions.OnlyOnFaulted)
            {
                _faultedContinuations.Add(continuationTask);
                if (_faulted) continuationTask.Start();
            } else if (continuationOptions == TaskContinuationOptions.None)
            {
                _continuations.Add(continuationTask);
                if (_completed || _faulted) continuationTask.Start();
            } else throw new NotImplementedException();
            return continuationTask;
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public static TaskFactory Factory
        {
            get { return new TaskFactory(); }
        }

        public void Wait()
        {
            _mre.WaitOne();
        }
    }

    public class Task<TResult> : Task
    {
        private readonly Func<TResult> _action;
        private TResult _result;


        public Task(Func<TResult> func, TaskFactory factory) : base(factory)
        {
            _action = func;
        }

        protected override void RunActualTask()
        {
            _result = _action();
        }


        public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
        {
            return HandleContinuationTask(_factory.ContinuationTask(this, continuationAction), continuationOptions);
        }

        public TResult Result
        {
            get { return _result; }
        }
    }


    [Serializable]
    [Flags]
    public enum TaskContinuationOptions
    {
        None = 0,
        NotOnFaulted = 16,
        OnlyOnRanToCompletion = 64,
        OnlyOnFaulted = 128,
    }

}