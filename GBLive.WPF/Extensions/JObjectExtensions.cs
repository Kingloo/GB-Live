using System;
using Newtonsoft.Json.Linq;

namespace GBLive.WPF.Extensions
{
    public static class JObjectExtensions
    {
        public static JObject ParseCatch<TException>(this string raw) where TException : Exception
            => ParseCatch<TException>(raw, null);

        public static JObject ParseCatch<TException>(this string raw, JsonLoadSettings settings) where TException : Exception
        {
            try
            {
                return JObject.Parse(raw, settings);
            }
            catch (TException)
            {
                return null;
            }
        }
    }
}
