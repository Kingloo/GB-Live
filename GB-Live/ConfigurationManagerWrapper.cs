using System;
using System.Configuration;

namespace GB_Live
{
    public static class ConfigurationManagerWrapper
    {
        public static bool TryGetString(string key, out string value)
        {
            string s = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(s))
            {
                value = null;
                return false;
            }
            else
            {
                value = s;
                return true;
            }
        }

        public static bool TryGetUri(string key, out Uri uri)
        {
            string value = string.Empty;

            if (TryGetString(key, out value))
            {
                return Uri.TryCreate(value, UriKind.Absolute, out uri);
            }
            else
            {
                uri = null;
                return false;
            }
        }
    }
}
