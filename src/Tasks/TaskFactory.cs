using System;
using System.Security.Permissions;

namespace Tasks
{
    [HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true,
    ExternalThreading = true)]
    public class TaskFactory
    {
        private readonly CancellationToken _token;

        public TaskFactory(CancellationToken token)
        {
            _token = token;
        }

        public TaskFactory()
        {
        }

        public CancellationToken CancellationToken
        {
            get {
                return _token;
            }
        }

        public Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, Object state)
        {
            Action innerAction = null;
            Task task = new Task(() => { if (innerAction != null) innerAction(); else throw new Exception("inneraction should never be null"); }, this);
            beginMethod(ar =>
                            {
                                innerAction = () => endMethod(ar);
                                task.Start();
                            }, state);
            return task;
        }

        public Task StartNew(Action action)
        {
            var task = new Task(action, this);
            task.Start();
            return task;
        }

        public Task<TResult> StartNew<TResult>(Func<TResult> result)
        {
            var task = new Task<TResult>(result, this);
            task.Start();
            return task;
        }


        public Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, Object state)
        {
            Func<TResult> innerFunc = null;
            var task = new Task<TResult>(() =>
                                             {
                                                 if (innerFunc != null)  return innerFunc();
                                                 throw new Exception("inneraction should never be null");
                                             }, this);
            beginMethod(ar =>
            {
                innerFunc = () => endMethod(ar);
                task.Start();
            }, state);
            return task;
        }

        public Task ContinuationTask<TResult>(Task<TResult> parentTask, Action<Task<TResult>> continuationAction)
        {
            return new Task(() => continuationAction(parentTask), this);
        }

        public Task ContinuationTask(Task parentTask, Action<Task> continuationAction)
        {
            return new Task(() => continuationAction(parentTask), this);
        }
    }
}
