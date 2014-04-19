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
    public partial class MainWindow : Window
    {
        private const string appName = "GB Live";
        private DispatcherTimer _updateTimer = new DispatcherTimer();
        public ObservableCollection<GBUpcomingEvent> Events { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            
            this.Events = new ObservableCollection<GBUpcomingEvent>();
            this.Events.CollectionChanged += Events_CollectionChanged;
            
            this._updateTimer.Tick += _updateTimer_Tick;
            this._updateTimer.Interval = new TimeSpan(0, 5, 0);
            this._updateTimer.IsEnabled = true;
        }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.Count > 0)
            {
                foreach (GBUpcomingEvent each in e.NewItems)
                {
                    each.StartCountdownTimer();
                }
            }
        }

        private async void _updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAsync();
        }

        private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                //case System.Windows.Input.Key.F1:
                //    Application.Current.Dispatcher.Invoke(new Action(
                //        delegate()
                //        {
                //            NotificationService.Send(this.Title, new Uri("http://www.giantbomb.com"));
                //        }), DispatcherPriority.SystemIdle);
                //    break;
                case System.Windows.Input.Key.F5:
                    await UpdateAsync();
                    break;
                case System.Windows.Input.Key.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        private async Task UpdateAsync()
        {
            this.Title = string.Format("{0}: checking ...", appName);
            
            string rawWebsite = await DownloadWebsite("http://www.giantbomb.com");

            if (!(String.IsNullOrEmpty(rawWebsite)))
            {
                string beginsUpcomingHtml = "<dl class=\"promo-upcoming\">";
                string endsUpcomingHtml = "</dl>";

                if (rawWebsite.Contains(beginsUpcomingHtml))
                {
                    string upcomingEventsHtml = RetrieveHtmlSegmentFromWholePage(rawWebsite, beginsUpcomingHtml, endsUpcomingHtml);

                    List<GBUpcomingEvent> allEvents = ParseHtmlForEvents(upcomingEventsHtml);

                    if (allEvents.Count == 0)
                    {
                        this.Events.Clear();
                    }
                    else
                    {
                        RemoveOldEvents(allEvents); // when an existing event has been removed from the website
                    }

                    AddNewEvents(allEvents);
                }
            }

            this.Title = appName;
        }

        private string RetrieveHtmlSegmentFromWholePage(string whole, string begins, string ends)
        {
            int startingIndex = whole.IndexOf(begins); // will return the index of the opening <
            int endingIndex = whole.IndexOf(ends, startingIndex); // will return the index of the opening <
            int length = endingIndex - startingIndex + 5; // + 5 to include all of the </dl> tag
            
            return whole.Substring(startingIndex, length);
        }

        private void RemoveOldEvents(List<GBUpcomingEvent> events)
        {
            List<GBUpcomingEvent> eventsToRemove = new List<GBUpcomingEvent>();

            foreach (GBUpcomingEvent eventInThisDotEvents in this.Events)
            {
                if (events.Contains(eventInThisDotEvents) == false)
                {
                    eventsToRemove.Add(eventInThisDotEvents);
                }
            }

            foreach (GBUpcomingEvent anEvent in eventsToRemove)
            {
                this.Events.Remove(anEvent);
            }
        }

        private void AddNewEvents(List<GBUpcomingEvent> events)
        {
            foreach (GBUpcomingEvent each in events)
            {
                if (EventNotFound(each))
                {
                    this.Events.Add(each);
                }
            }
        }

        private async Task<string> DownloadWebsite(string gbUrl)
        {
            string response = string.Empty;

            HttpWebRequest req = BuildHttpWebRequest(gbUrl);
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

        private HttpWebRequest BuildHttpWebRequest(string url)
        {
            Uri gbUri = new Uri(url);

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
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:28.0) Gecko/20100101 Firefox/28.0"; // Firefox 28 on Win8.1 64bit

            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(gbUri, new Cookie("xcg", "lu")); // maybe is my country, perhaps for timezone purposes

            return req;
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

        private bool EventNotFound(GBUpcomingEvent eventToCheck)
        {
            foreach (GBUpcomingEvent each in this.Events)
            {
                if (each.Equals(eventToCheck))
                {
                    return false;
                }
            }

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Really close?", "GB Live", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
