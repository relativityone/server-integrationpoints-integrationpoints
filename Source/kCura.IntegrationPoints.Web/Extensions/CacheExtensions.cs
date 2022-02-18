﻿using System;
using System.Web.Caching;

namespace kCura.IntegrationPoints.Web.Extensions
{
    public static class CacheExtensions
    {
        public static T GetOrInsert<T>(this Cache cache, string key, Func<T> valueFactory, TimeSpan slidingExpiration)
        {
            object value = cache[key];

            if (value == null)
            {
                value = valueFactory();
                cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration);
            }

            return (T)value;
        }
    }
}