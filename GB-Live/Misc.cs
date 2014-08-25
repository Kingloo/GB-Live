﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GB_Live
{
    /// <summary>
    /// Miscellaneous functions and extensions.
    /// </summary>
    public static class Misc
    {
        /// <summary>
        /// Trys to navigate to a string Url address in the default browser, then in Internet Explorer.
        /// </summary>
        /// <param name="urlString">The address as a string.</param>
        public static void OpenUrlInBrowser(string urlString)
        {
            Uri uri = null;

            if (Uri.TryCreate(urlString, UriKind.Absolute, out uri))
            {
                System.Diagnostics.Process.Start(uri.AbsoluteUri);
            }
            else
            {
                throw new ArgumentException("urlString passed to Uri.TryCreate returns false");
            }
        }

        /// <summary>
        /// Trys to navigate to a Uri address in the default browser, then in Internet Explorer.
        /// </summary>
        /// <param name="uri">The Uri object.</param>
        public static void OpenUrlInBrowser(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                System.Diagnostics.Process.Start(uri.AbsoluteUri);
            }
            else
            {
                throw new ArgumentException("uri is not absolute");
            }
        }

        /// <summary>
        /// You want a string.Contains that you can tell about case and culture.
        /// </summary>
        /// <param name="target">The string to search within.</param>
        /// <param name="toFind">The string you want to find.</param>
        /// <param name="comp">A StringComparison used to determine Culture and Case options.</param>
        /// <returns>A boolean, TRUE means target does contain find and vice versa.</returns>
        public static bool ContainsExt(this string target, string toFind, StringComparison comp)
        {
            if (target.IndexOf(toFind, comp) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Logs a message to logfile.txt in user's Documents directory.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogMessage(string message)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} logged the following message at {1}:", Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(message);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, false))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Asynchronously logs a message to logfile.txt in user's Documents directory.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static async Task LogMessageAsync(string message)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} logged the following message at {1}:", Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(message);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Logs a DispatcherUnhandledException's Exception property to a file.
        /// </summary>
        /// <param name="e">The exception object within the DispatcherUnhandledException.</param>
        public static void LogException(Exception e)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(e.Message);
            logMessage.AppendLine(e.StackTrace);

            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, false))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Asynchronously logs an Exception to a file.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        public async static Task LogExceptionAsync(Exception e)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(e.Message);
            logMessage.AppendLine(e.StackTrace);

            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Adds each item from an IEnumerable to a Collection.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The Collection to add the items to.</param>
        /// <param name="list">The IEnumerable of type T to add to the Collection.</param>
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Add(obj);
            }
        }

        /// <summary>
        /// Adds all items that aren't already present in the Collection.
        /// We enforce that T implement IEquatable T because otherwise Collection T.Contains uses Object.Contains,
        /// which is only a reference equals.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The collection to add the items to.</param>
        /// <param name="list">The IEnumerable of T to add.</param>
        public static void AddMissingItems<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            foreach (T each in list)
            {
                if (collection.Contains(each) == false)
                {
                    collection.Add(each);
                }
            }
        }

        /// <summary>
        /// Adds all items that aren't already present in the Collection.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The collection to add the items to.</param>
        /// <param name="list">The IEnumerable of T to add.</param>
        /// <param name="comparer">The IEnumerable of T to add.</param>
        public static void AddMissingItems<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            foreach (T each in list)
            {
                if (collection.Contains<T>(each, comparer) == false)
                {
                    collection.Add(each);
                }
            }
        }

        /// <summary>
        /// Returns a HttpWebResponse that deals with WebException inside.
        /// </summary>
        /// <param name="req">The HttpWebRequest to perform.</param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> GetResponseAsyncExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

            try
            {
                webResp = await req.GetResponseAsync();
            }
            catch (WebException e)
            {
                webResp = e.Response;

                if (webResp == null)
                {
                    Misc.LogMessage("GetResponseAsyncExt: webResp was null");

                    throw;
                }
            }

            return (HttpWebResponse)webResp;
        }

        /// <summary>
        /// You have a string[] where each string ends in a number. Typical ordering will give you 0,1,10,...,2,20 etc. But you want 0,1,2,...,10,11,...20,21 etc.
        /// </summary>
        /// <param name="list">A string[] ordered incorrectly.</param>
        /// <param name="d">Positive int means 0,1,2,3..., negative int means 5,4,3,2,1,0, and 0 fails.</param>
        /// <returns>A string[] ordered properly.</returns>
        public static string[] OrderStringArray(this string[] list, int d)
        {
            int numberOfStrings = list.Count<string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();

            foreach (string str in list)
            {
                dict.Add(getNumber(str), str);
            }

            IEnumerable<KeyValuePair<int, string>> ienum = null;

            if (d > 0)
            {
                ienum = dict.OrderBy(a => a.Key);
            }
            else if (d < 0)
            {
                ienum = dict.OrderByDescending(a => a.Key);
            }
            else
            {
                return new string[0];
            }

            string[] sorted = new string[dict.Count];

            int i = 0;
            foreach (KeyValuePair<int, string> kvp in ienum)
            {
                sorted[i] = kvp.Value;
                i++;
                Console.WriteLine("Dict: " + kvp.Key.ToString() + ", " + kvp.Value);
            }

            return sorted;
        }

        private static int getNumber(string str)
        {
            int index = 0;

            for (int i = str.Length - 1; i > 0; i--)
            {
                if (Char.IsLetter(str[i]) == true)
                {
                    index = i + 1;
                    break;
                }
            }

            return Convert.ToInt32(str.Substring(index));
        }

        /// <summary>
        /// Sets a window to the centre of the primary screen.
        /// </summary>
        /// <param name="window">The window you want to centre.</param>
        public static void SetWindowToMiddleOfScreen(Window window)
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }
    }
	
	/// <summary>
    /// Basic EventArgs class with only a string parameter.
    /// </summary>
	public class MessageEventArgs : EventArgs
	{
		public string Message { get; private set; }

        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
	}
}