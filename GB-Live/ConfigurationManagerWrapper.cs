using System;
using System.Configuration;
using System.Globalization;

namespace GB_Live
{
    public static class ConfigurationManagerWrapper
    {
        public static string GetStringOrThrow(string key)
        {
            string s = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(s))
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Key \"{0}\" was not found", key);

                throw new ConfigurationErrorsException(errorMessage);
            }
            else
            {
                return s;
            }
        }

        public static Uri GetUriOrThrow(string key)
        {
            string s = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(s))
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Key \"{0}\" was not found", key);

                throw new ConfigurationErrorsException(errorMessage);
            }
            else
            {
                return Uri.TryCreate(s, UriKind.Absolute, out Uri uri) ? uri : null;
            }
        }

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
            if (TryGetString(key, out string value))
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
