using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GB_Live
{
    public enum Result
    {
        None,
        Success,
        BeginningNotFound,
        BeginningNotUnique,
        EndingNotFound,
        EndingBeforeBeginning
    };

    public class FromBetweenResult
    {
        private Result _result = Result.None;
        public Result Result
        {
            get
            {
                return _result;
            }
        }

        private string _resultString = string.Empty;
        public string ResultString
        {
            get
            {
                return _resultString;
            }
        }

        public FromBetweenResult(Result result, string resultString)
        {
            this._result = result;
            this._resultString = resultString;
        }
    }

    public static class Extensions
    {
        // string
        public static bool ContainsExt(this string target, string toFind, StringComparison comparison)
        {
            return target.IndexOf(toFind, comparison) > -1;
        }

        public static string RemoveNewLines(this string s)
        {
            string toReturn = s;

            if (toReturn.Contains("\r\n"))
            {
                toReturn = toReturn.Replace("\r\n", " ");
            }

            if (toReturn.Contains("\r"))
            {
                toReturn = toReturn.Replace("\r", " ");
            }

            if (toReturn.Contains("\n"))
            {
                toReturn = toReturn.Replace("\n", " ");
            }

            if (toReturn.Contains(Environment.NewLine))
            {
                toReturn = toReturn.Replace(Environment.NewLine, " ");
            }

            return toReturn;
        }

        public static FromBetweenResult FromBetween(this string whole, string beginning, string ending)
        {
            if (whole.Contains(beginning) == false)
            {
                return new FromBetweenResult(Result.BeginningNotFound, string.Empty);
            }

            int firstAppearanceOfBeginning = whole.IndexOf(beginning);
            int lastAppearanceOfBeginning = whole.LastIndexOf(beginning);

            if (firstAppearanceOfBeginning != lastAppearanceOfBeginning)
            {
                return new FromBetweenResult(Result.BeginningNotUnique, string.Empty);
            }

            int indexOfBeginning = firstAppearanceOfBeginning;

            if (whole.Contains(ending) == false)
            {
                return new FromBetweenResult(Result.EndingNotFound, string.Empty);
            }

            int indexOfEnding = 0;

            try
            {
                indexOfEnding = whole.IndexOf(ending, indexOfBeginning);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new FromBetweenResult(Result.EndingBeforeBeginning, string.Empty);
            }
            
            int indexOfResult = indexOfBeginning + beginning.Length;
            int lengthOfResult = indexOfEnding - indexOfResult;

            string result = whole.Substring(indexOfResult, lengthOfResult);

            return new FromBetweenResult(Result.Success, result);
        }


        // ICollection<T>
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Add(obj);
            }
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            int countAdded = 0;

            foreach (T obj in list)
            {
                if (collection.Contains(obj) == false)
                {
                    collection.Add(obj);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            int countAdded = 0;

            foreach (T obj in list)
            {
                if (collection.Contains<T>(obj, comparer) == false)
                {
                    collection.Add(obj);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static void RemoveList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Remove(obj);
            }
        }


        // HttpWebRequest
        public static WebResponse GetResponseExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

            try
            {
                webResp = req.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }

                string message = string.Format("Request uri: {0}, Method: {1}, Timeout: {2}", req.RequestUri, req.Method, req.Timeout);

                Utils.LogException(e, message);
            }

            return webResp;
        }
        
        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;
            
            try
            {
                webResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }

                string message = string.Format("Request uri: {0}, Method: {1}, Timeout: {2}", req.RequestUri, req.Method, req.Timeout);

                Utils.LogException(e, message); // C#6 will allow for awaiting in Catch/Finally blocks
            }

            return webResp;
        }


        // Task
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)) == false)
            {
                throw new TimeoutException(string.Format("Task timed out: {0}", task.Status.ToString()));
            }

            //if (task == Task.WhenAny(task, Task.Delay(timeout)))
            //{
            //    await task;
            //}
            //else
            //{
            //    throw new TimeoutException(string.Format("Task timed out: {0}", task.Status.ToString()));
            //}
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return task.Result;
            }
            else
            {
                throw new TimeoutException(string.Format("Task timed out: {0}", task.Status.ToString()));
            }
        }
    }
}