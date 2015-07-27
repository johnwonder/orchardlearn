using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace Orchard.WarmupStarter {
    public class WarmupHttpModule : IHttpModule {
        private HttpApplication _context;
        private static object _synLock = new object();
        private static IList<Action> _awaiting = new List<Action>();

        public void Init(HttpApplication context) {
            _context = context;
            context.AddOnBeginRequestAsync(BeginBeginRequest, EndBeginRequest, null);
        }

        public void Dispose() {
        }

        private static bool InWarmup() {
            lock (_synLock) {
                return _awaiting != null;
            }
        }

        /// <summary>
        /// Warmup code is about to start: Any new incoming request is queued 
        /// until "SignalWarmupDone" is called.
        /// </summary>
        public static void SignalWarmupStart() {
            lock (_synLock) {
                if (_awaiting == null) {
                    _awaiting = new List<Action>();
                }
            }
        }

        /// <summary>
        /// Warmup code just completed: All pending requests in the "_await" queue are processed, 
        /// and any new incoming request is now processed immediately.
        /// </summary>
        public static void SignalWarmupDone() {
            IList<Action> temp;

            lock (_synLock) {
                temp = _awaiting;
                _awaiting = null;
            }

            if (temp != null) {
                foreach (var action in temp) {
                    action();
                }
            }
        }

        /// <summary>
        /// Enqueue or directly process action depending on current mode.
        /// </summary>
        private void Await(Action action) {
            Action temp = action;

            lock (_synLock) {
                if (_awaiting != null) {
                    temp = null;
                    _awaiting.Add(action);
                }
            }

            if (temp != null) {
                temp();
            }
        }
        //http://www.cnblogs.com/springyangwc/archive/2011/10/12/2208991.html
        //A、AutoResetEvent.WaitOne()每次只允许一个线程进入，当某个线程得到信号后，AutoResetEvent会自动又将信号置为不发送状态，则其他调用WaitOne的线程只有继续等待，也就是说AutoResetEvent一次只唤醒一个线程； 
        //B、ManualResetEvent则可以唤醒多个线程，因为当某个线程调用了ManualResetEvent.Set()方法后，其他调用WaitOne的线程获得信号得以继续执行，而ManualResetEvent不会自动将信号置为不发送。

        //C、也就是说，除非手工调用了ManualResetEvent.Reset()方法，则ManualResetEvent将一直保持有信号状态，ManualResetEvent也就可以同时唤醒多个线程继续执行。
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="cb"></param>
        /// <param name="extradata"></param>
        /// <returns></returns>
        private IAsyncResult BeginBeginRequest(object sender, EventArgs e, AsyncCallback cb, object extradata) {
            // host is available, process every requests, or file is processed
            if (!InWarmup() || WarmupUtility.DoBeginRequest(_context)) {
                var asyncResult = new DoneAsyncResult(extradata);
                cb(asyncResult);
                return asyncResult;
            }
            else {
                // this is the "on hold" execution path
                var asyncResult = new WarmupAsyncResult(cb, extradata);
                Await(asyncResult.Completed);
                return asyncResult;
            }
        }

        private static void EndBeginRequest(IAsyncResult ar) {
        }

        /// <summary>
        /// AsyncResult for "on hold" request (resumes when "Completed()" is called)
        /// </summary>
        private class WarmupAsyncResult : IAsyncResult {
            //http://www.cnblogs.com/lzjsky/archive/2011/07/11/2102794.html AutoResetEvent
            private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false/*initialState*/);
            private readonly AsyncCallback _cb;
            private readonly object _asyncState;
            private bool _isCompleted;

            public WarmupAsyncResult(AsyncCallback cb, object asyncState) {
                _cb = cb;
                _asyncState = asyncState;
                _isCompleted = false;
            }

            public void Completed() {
                _isCompleted = true;
             //   _eventWaitHandle.Set();
                _cb(this);
            }

            bool IAsyncResult.CompletedSynchronously {
                get { return false; }
            }

            bool IAsyncResult.IsCompleted {
                get { return _isCompleted; }
            }

            object IAsyncResult.AsyncState {
                get { return _asyncState; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle {
                get { return _eventWaitHandle; }
            }
        }

        /// <summary>
        /// Async result for "ok to process now" requests
        /// </summary>
        private class DoneAsyncResult : IAsyncResult {
            private readonly object _asyncState;
            //http://www.cnblogs.com/tianzhiliang/archive/2011/03/04/1970726.html
            //ManualResetEvent 允许线程通过发信号互相通信。通常，此通信涉及一个线程在其他线程进行之前必须完成的任务。当一个线程开始一个活动（此活动必须完成后，其他线程才能开始）时，它调用 Reset 以将 ManualResetEvent 置于非终止状态，此线程可被视为控制 ManualResetEvent。调用 ManualResetEvent 上的 WaitOne 的线程将阻止，并等待信号。当控制线程完成活动时，它调用 Set 以发出等待线程可以继续进行的信号。并释放所有等待线程。一旦它被终止，ManualResetEvent 将保持终止状态（即对 WaitOne 的调用的线程将立即返回，并不阻塞），直到它被手动重置。可以通过将布尔值传递给构造函数来控制 ManualResetEvent 的初始状态，如果初始状态处于终止状态，为 true；否则为 false。
            private static readonly WaitHandle _waitHandle = new ManualResetEvent(true/*initialState*/);//处于终止状态

            public DoneAsyncResult(object asyncState) {
                _asyncState = asyncState;
            }

            bool IAsyncResult.CompletedSynchronously {
                get { return true; }
            }

            bool IAsyncResult.IsCompleted {
                get { return true; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle {
                get { return _waitHandle; }
            }

            object IAsyncResult.AsyncState {
                get { return _asyncState; }
            }
        }
    }
}