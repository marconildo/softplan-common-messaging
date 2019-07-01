using System;
using System.Threading;
using Pose;

namespace Softplan.Common.Messaging.Tests.Helper
{
    public static class PoseSemaphoreHelper
    {
        private static readonly Semaphore Semaphore = new Semaphore(1, 1);
        
        public static void Isolate(Action entryPoint, params Shim[] shims)
        {
            Semaphore.WaitOne();
            PoseContext.Isolate(entryPoint, shims);
            Semaphore.Release();
        }
    }
}