using System;

namespace Peshitta.Infrastructure.Attributes
{
    /// <summary>
    /// causes the specified data to be cached for indicated timespan
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CacheAttribute: Attribute
    {
      
    }
}
