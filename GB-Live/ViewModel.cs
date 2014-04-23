using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GB_Live
{
    class ViewModel : ViewModelBase
    {
        private const string appName = "GB Live";
        private const string upcomingBegins = "<dl class=\"promo-upcoming\">";
        private const string upcomingEnds = "</dl>";

        private DispatcherTimer _updateTimer = new DispatcherTimer();
        private bool _isLive = false;
        public bool IsLive
        {
            get { return this._isLive; }
            set
            {
                this._isLive = value;
                this.OnPropertyChanged("IsLive");
            }
        }
        private string _windowTitle = appName;
        public string WindowTitle
        {
            get { return this._windowTitle; }
            set
            {
                this._windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }
        public ObservableCollection<GBUpcomingEvent> Events { get; private set; }

        public ViewModel()
        {
            this.IsLive = false;

            this.Events = new ObservableCollection<GBUpcomingEvent>();
            this.Events.CollectionChanged += Events_CollectionChanged;

            this._updateTimer.Tick += _updateTimer_Tick;
            this._updateTimer.Interval = new TimeSpan(0, 5, 0);
            this._updateTimer.IsEnabled = true;
        }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    if (e.NewItems.Count > 0)
                    {
                        foreach (GBUpcomingEvent each in e.NewItems)
                        {
                            each.StartCountdownTimer();
                        }
                    }
                }
            }
        }
        
        private async void _updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAsync();
        }

        public async Task UpdateAsync()
        {
            this.WindowTitle = string.Format("{0}: checking ...", appName);

            Task[] taskArray = new Task[2];
            taskArray[0] = CheckForLiveShow();
            taskArray[1] = UpdateUpcomingEvents();

            await Task.WhenAll(taskArray);

            this.WindowTitle = appName;
        }

        private async Task CheckForLiveShow()
        {
            Uri gbChat = ((UriBuilder)Application.Current.Resources["gbChat"]).Uri;
            string websiteAsString = await DownloadWebsite(gbChat);

            if (!(String.IsNullOrEmpty(websiteAsString)))
            {
                if (websiteAsString.Contains("There is currently no show"))
                {
                    this.IsLive = false;
                }
                else
                {
                    if (this.IsLive == false)
                    {
                        NotificationService.Send("Giantbomb is LIVE", gbChat);
                    }

                    this.IsLive = true;
                }
            }
        }

        private async Task UpdateUpcomingEvents()
        {
            Uri gbHome = ((UriBuilder)Application.Current.Resources["gbHome"]).Uri;
            string websiteAsString = await DownloadWebsite(gbHome);

            if (!(String.IsNullOrEmpty(websiteAsString)))
            {
                List<GBUpcomingEvent> eventsFromHtml = RetrieveEventsFromHtml(websiteAsString);

                if (eventsFromHtml.Count == 0)
                {
                    this.Events.Clear();
                }
                else
                {
                    RemoveOldEvents();
                    AddNewEvents(eventsFromHtml);
                }
            }
        }

        private List<GBUpcomingEvent> RetrieveEventsFromHtml(string websiteAsString)
        {
            string upcomingHtml = FindUpcomingHtml(websiteAsString);

            return ParseHtmlForEvents(upcomingHtml);
        }

        private string FindUpcomingHtml(string websiteAsString)
        {
            int index_upcomingBegins = websiteAsString.IndexOf(upcomingBegins);
            int index_upcomingEnds = websiteAsString.IndexOf(upcomingEnds, index_upcomingBegins);
            int length_upcomingHtml = index_upcomingEnds - index_upcomingBegins;

            if (websiteAsString.Contains(upcomingBegins) == false)
            {
                return string.Empty;
            }

            if (index_upcomingBegins < 0)
            {
                return string.Empty;
            }

            if (index_upcomingEnds < 0)
            {
                return string.Empty;
            }

            if (index_upcomingEnds <= index_upcomingBegins)
            {
                return string.Empty;
            }

            return websiteAsString.Substring(index_upcomingBegins, length_upcomingHtml);
        }

        private List<GBUpcomingEvent> ParseHtmlForEvents(string s)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();

            string dd = string.Empty;
            bool shouldConcat = false;

            using (StringReader sr = new StringReader(s))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("<dd"))
                    {
                        shouldConcat = true;
                    }
                    else if (line.StartsWith("</dd"))
                    {
                        shouldConcat = false;
                        events.Add(new GBUpcomingEvent(dd));
                        dd = string.Empty;
                    }

                    if (shouldConcat)
                    {
                        dd += line;
                    }
                }
            }

            return events;
        }

        private void RemoveOldEventsDEPRECATED(List<GBUpcomingEvent> eventsInHtml)
        {
            List<GBUpcomingEvent> eventsToRemove = new List<GBUpcomingEvent>();

            foreach (GBUpcomingEvent eventInThisDotEvents in this.Events)
            {
                // if a given event in the ViewModel OC is not in the list
                // of events we built from the html, we want to remove it
                // however, we can't remove it from the collection during a foreach
                // so we add it to a temporary list
                if (EventNotFoundInList(eventInThisDotEvents, eventsInHtml))
                {
                    eventsToRemove.Add(eventInThisDotEvents);
                }
            }

            // now we iterate the temporary list and remove every one from the ViewModel OC
            foreach (GBUpcomingEvent anEvent in eventsToRemove)
            {
                this.Events.Remove(anEvent);
            }
        }

        private void RemoveOldEvents()
        {
            List<GBUpcomingEvent> eventsToRemove = new List<GBUpcomingEvent>();

            foreach (GBUpcomingEvent each in this.Events)
            {
                if (each.Time.Equals(DateTime.MaxValue))
                {
                    eventsToRemove.Add(each);
                }
            }

            foreach (GBUpcomingEvent each in eventsToRemove)
            {
                this.Events.Remove(each);
            }
        }

        private void AddNewEvents(List<GBUpcomingEvent> eventsInHtml)
        {
            List<GBUpcomingEvent> eventsWeAlreadyHave = new List<GBUpcomingEvent>(this.Events);

            foreach (GBUpcomingEvent each in eventsInHtml)
            {
                if (EventNotFoundInList(each, eventsWeAlreadyHave))
                {
                    this.Events.Add(each);
                }
            }
        }

        private static bool EventNotFoundInList(GBUpcomingEvent eventToCheck, List<GBUpcomingEvent> listToCheckWithin)
        {
            foreach (GBUpcomingEvent each in listToCheckWithin)
            {
                if (listToCheckWithin.Contains(each))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<string> DownloadWebsite(Uri gbUri)
        {
            string response = string.Empty;

            HttpWebRequest req = BuildHttpWebRequest(gbUri);
            WebResponse webResp = null;

            try
            {
                webResp = await req.GetResponseAsync();
            }
            catch (WebException)
            {
                if (webResp != null)
                {
                    webResp.Close();
                }

                return string.Empty;
            }

            if (webResp != null)
            {
                using (StreamReader sr = new StreamReader(webResp.GetResponseStream()))
                {
                    response = await sr.ReadToEndAsync();
                }

                webResp.Close();
            }

            return response;
        }

        private HttpWebRequest BuildHttpWebRequest(Uri gbUri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(gbUri);

            req.Accept = "text/html";
            req.Headers.Add("Accept-Language", "en-gb");
            req.Headers.Add("DNT", "1");
            req.Host = gbUri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version10;
            req.Referer = gbUri.DnsSafeHost;
            req.ServicePoint.ConnectionLimit = 1;
            req.Timeout = 750;
            // Firefox 28 on Win 8.1 64bit
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:28.0) Gecko/20100101 Firefox/28.0";

            // lu is probably country-code, returned timezone has yet to be wrong after setting this
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(gbUri, new Cookie("xcg", "lu"));

            return req;
        }

        public void NavigateToChatPage()
        {
            Uri gbChat = ((UriBuilder)Application.Current.Resources["gbChat"]).Uri;
            Misc.OpenUrlInBrowser(gbChat);
        }
    }
}
