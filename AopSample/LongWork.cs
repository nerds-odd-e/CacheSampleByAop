using System;
using System.Threading;
using Autofac.Extras.DynamicProxy;

namespace AopSample
{
    [Intercept(typeof(CacheResultInterceptor))]
    public class LongWork : ILongWork
    {
        [CacheResult(Duration = 3500)]
        public string Process(int first, int second)
        {
            Console.WriteLine($"sleep 1.5 seconds, first:{first}, second:{second}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }
}