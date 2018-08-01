using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;

namespace AopSample
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            ContainerRegister();

            //var longWork = new LongWork();
            var longWork = _container.Resolve<ILongWork>();
            Console.WriteLine(longWork.Process(1, 2));
            Console.WriteLine();
            Console.WriteLine(longWork.Process(1, 2));
            Console.WriteLine();

            Console.WriteLine(longWork.Process(3, 4));
            Console.WriteLine();

            Console.WriteLine(longWork.Process(1, 2));
            Console.WriteLine();

            Console.WriteLine(longWork.Process(3, 4));
            Console.WriteLine();
            Console.WriteLine(longWork.Process(3, 4));
        }

        private static void ContainerRegister()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<MemoryCacheProvider>()
                .As<ICacheProvider>()
                .SingleInstance();

            containerBuilder.RegisterType<CacheResultInterceptor>()
                .SingleInstance();

            containerBuilder.RegisterType<LongWork>()
                .As<ILongWork>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(CacheResultInterceptor));

            _container = containerBuilder.Build();
        }
    }

    public class CacheResultInterceptor : IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheResultInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public CacheResultAttribute GetCacheResultAttribute(IInvocation invocation)
        {
            return Attribute.GetCustomAttribute(
                    invocation.MethodInvocationTarget,
                    typeof(CacheResultAttribute)
                )
                as CacheResultAttribute;
        }

        public string GetInvocationSignature(IInvocation invocation)
        {
            return String.Format("{0}-{1}-{2}",
                invocation.TargetType.FullName,
                invocation.Method.Name,
                String.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())
            );
        }

        public void Intercept(IInvocation invocation)
        {
            var cacheAttr = GetCacheResultAttribute(invocation);

            if (cacheAttr == null)
            {
                invocation.Proceed();
                return;
            }

            string key = GetInvocationSignature(invocation);

            if (_cache.Contains(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return;
            }

            invocation.Proceed();
            var result = invocation.ReturnValue;

            if (result != null)
            {
                _cache.Put(key, result, cacheAttr.Duration);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheResultAttribute : Attribute
    {
        public int Duration { get; set; }
    }

    public interface ICacheProvider
    {
        object Get(string key);

        void Put(string key, object value, int duration);

        bool Contains(string key);
    }

    public class MemoryCacheProvider : ICacheProvider
    {
        public object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public void Put(string key, object value, int duration)
        {
            if (duration <= 0)
                throw new ArgumentException("Duration cannot be less or equal to zero", nameof(duration));

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(duration)
            };

            MemoryCache.Default.Set(key, value, policy);
        }

        public bool Contains(string key)
        {
            return MemoryCache.Default[key] != null;
        }
    }

    [Intercept(typeof(CacheResultInterceptor))]
    public class LongWork : ILongWork
    {
        [CacheResult(Duration = 5000)]
        public string Process(int first, int second)
        {
            Console.WriteLine($"sleep 1.5 seconds, first:{first}, second:{second}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }

    public interface ILongWork
    {
        string Process(int first, int second);
    }
}
