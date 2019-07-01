using System;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmtracer : ITracer
    {        
        public ITransaction CurrentTransaction => Agent.Tracer.CurrentTransaction;

        public void CaptureTransaction(
            string name, 
            string type, 
            Action<ITransaction> action, 
            DistributedTracingData distributedTracingData = null)
        {
            Agent.Tracer.CaptureTransaction(name, type, action, distributedTracingData);
        }

        public void CaptureTransaction(
            string name,
            string type,
            Action action,
            DistributedTracingData distributedTracingData = null)
        {
            Agent.Tracer.CaptureTransaction(name, type, action, distributedTracingData);
        }

        public T CaptureTransaction<T>(
            string name, 
            string type, 
            Func<ITransaction, T> func, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }

        public T CaptureTransaction<T>(
            string name,
            string type,
            Func<T> func,
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }

        public Task CaptureTransaction(
            string name, 
            string type, 
            Func<Task> func, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }

        public Task CaptureTransaction(
            string name, 
            string type, 
            Func<ITransaction, Task> func, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }

        public Task<T> CaptureTransaction<T>(
            string name, 
            string type, Func<Task<T>> func, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }

        public Task<T> CaptureTransaction<T>(
            string name, string type, 
            Func<ITransaction, Task<T>> func, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.CaptureTransaction(name, type, func, distributedTracingData);
        }
        
        public ITransaction StartTransaction(
            string name, 
            string type, 
            DistributedTracingData distributedTracingData = null)
        {
            return Agent.Tracer.StartTransaction(name, type, distributedTracingData);
        }
    }
}