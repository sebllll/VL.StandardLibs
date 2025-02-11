﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace VL.Core
{
    /// <summary>
    /// A collection of methods used by nodes which introduce new entry points into the VL runtime graph.
    /// </summary>
    public static class RuntimeGraph
    {
        [ThreadStatic]
        private static int s_rethrowExceptions;

        static RuntimeGraph()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var allHandled = true;
            foreach (var ex in e.Exception.InnerExceptions)
            {
                var re = ex as RuntimeException;
                if (re is null || !ReportException(re))
                    allHandled = false;
            }
            if (allHandled)
                e.SetObserved();
        }

        internal readonly struct Frame : IDisposable
        {
            private readonly int _previousState;

            public Frame(int previousState)
            {
                this._previousState = previousState;
            }

            public void Dispose()
            {
                s_rethrowExceptions = _previousState;
            }
        }

        internal static Frame EnableExceptionRethrow()
        {
            var previous = Interlocked.Exchange(ref s_rethrowExceptions, 1);
            return new Frame(previous);
        }

        internal static bool RethrowExceptions() => s_rethrowExceptions == 1;

        /// <summary>
        /// The exception to throw by the HandleAsyncException call.
        /// </summary>
        /// <param name="exception">The exception to report.</param>
        public static bool ReportException(Exception exception)
        {
            return ReportException(exception, AppHost.CurrentOrGlobal);
        }

        /// <summary>
        /// The exception to throw by the HandleAsyncException call.
        /// </summary>
        /// <param name="exception">The exception to report.</param>
        /// <param name="capturedAppHost">The captured service registry.</param>
        public static bool ReportException(Exception exception, AppHost capturedAppHost)
        {
            var appHost = AppHost.IsAnyCurrent() ? AppHost.Current : capturedAppHost;
            var runtime = appHost?.Services.GetService<IVLRuntime>();
            if (runtime != null)
            {
                runtime.ReportException(exception);
                return true;
            }
            return false;
        }
    }
}