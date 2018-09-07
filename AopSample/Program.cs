using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;

namespace AopSample
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            ContainerRegister();

            var longWork = new LongWork();
            //var longWork = _container.Resolve<ILongWork>();
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

            //Thread.Sleep(3000);

            //Console.WriteLine(longWork.Process(1, 2));
            //Console.WriteLine(longWork.Process(3, 4));
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
                .EnableInterfaceInterceptors();

            _container = containerBuilder.Build();
        }
    }
}
