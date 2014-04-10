using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

            this._updateTimer.Tick += _updateTimer_Tick;
            this._updateTimer.Interval = new TimeSpan(0, 0, 60);
            this._updateTimer.IsEnabled = true;

            this.Events = new ObservableCollection<GBUpcomingEvent>();
        }

        private async void _updateTimer_Tick(object sender, EventArgs e)
        {
            Console.Write("checking ... ");
            await UpdateAsync();
            Console.Write("finished\n");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAsync();
        }

        private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
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

            if (String.IsNullOrEmpty(rawWebsite))
            {
                MessageBox.Show("fail 1");
            }
            else
            {
                string beginsUpcomingHtml = "<dl class=\"promo-upcoming\">";
                string endsUpcomingHtml = "</dl>";

                if (rawWebsite.Contains(beginsUpcomingHtml))
                {
                    int startingIndex = rawWebsite.IndexOf(beginsUpcomingHtml); // will return the index of the opening <
                    int endingIndex = rawWebsite.IndexOf(endsUpcomingHtml, startingIndex); // will return the index of the opening <
                    int length = endingIndex - startingIndex + 5; // + 5 includes all of the </dl> tag

                    string upcomingHtml = rawWebsite.Substring(startingIndex, length);

                    List<GBUpcomingEvent> events = ParseHtmlForEvents(upcomingHtml);

                    foreach (GBUpcomingEvent each in events)
                    {
                        if (EventNotFound(each))
                        {
                            this.Events.Add(each);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("fail 2");
                }
            }

            this.Title = appName;
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
                webResp = null;
            }

            if (webResp != null)
            {
                using (StreamReader sr = new StreamReader(webResp.GetResponseStream()))
                {
                    response = await sr.ReadToEndAsync();
                }
            }

            return response;
        }

        private HttpWebRequest BuildHttpWebRequest(string url)
        {
            Uri gbUri = new Uri(url);

            HttpWebRequest req = HttpWebRequest.CreateHttp(gbUri);

            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version10;
            req.Referer = gbUri.DnsSafeHost;
            req.ServicePoint.ConnectionLimit = 1;
            req.Timeout = 750;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

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

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine("image failed: " + sender.GetType().ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Really close?", "GB Live", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image image = (Image)sender;

            Console.WriteLine(image.Source.Height + ", " + image.Source.Width);

            double amountTaller = image.Source.Height - 100d;
            double amountWider = image.Source.Width - 450d;

            if (amountTaller > 0)
            {
                image.SetValue(Canvas.TopProperty, -10d);
            }

            if (amountWider > 0)
            {
                image.SetValue(Canvas.LeftProperty, -50d);
            }
        }
    }
}
