using System;

namespace AopSample
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheResultAttribute : Attribute
    {
        public int Duration { get; set; }
    }
}