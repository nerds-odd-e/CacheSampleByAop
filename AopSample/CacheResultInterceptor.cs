using System;
using System.Linq;
using Castle.DynamicProxy;

namespace AopSample
{
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
}