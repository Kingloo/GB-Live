﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GB_Live
{
    public class ViewModel : ViewModelBase
    {
        #region Commands
        private DelegateCommandAsync _refreshCommandAsync = null;
        public DelegateCommandAsync RefreshCommandAsync
        {
            get
            {
                if (this._refreshCommandAsync == null)
                {
                    this._refreshCommandAsync = new DelegateCommandAsync(RefreshAsync, canExecuteAsync);
                }

                return this._refreshCommandAsync;
            }
        }

        private async Task RefreshAsync()
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        private DelegateCommand _goToHomePageInBrowserCommand = null;
        public DelegateCommand GoToHomePageInBrowserCommand
        {
            get
            {
                if (this._goToHomePageInBrowserCommand == null)
                {
                    this._goToHomePageInBrowserCommand = new DelegateCommand(GoToHomePageInBrowser, canExecute);
                }

                return this._goToHomePageInBrowserCommand;
            }
        }

        private void GoToHomePageInBrowser()
        {
            Utils.OpenUriInBrowser(Globals.gbHome);
        }

        private DelegateCommand _goToChatPageInBrowserCommand = null;
        public DelegateCommand GoToChatPageInBrowserCommand
        {
            get
            {
                if (this._goToChatPageInBrowserCommand == null)
                {
                    this._goToChatPageInBrowserCommand = new DelegateCommand(GoToChatPageInBrowser, canExecute);
                }

                return this._goToChatPageInBrowserCommand;
            }
        }

        private void GoToChatPageInBrowser()
        {
            Utils.OpenUriInBrowser(Globals.gbChat);
        }

        private DelegateCommand _exitCommand = null;
        public DelegateCommand ExitCommand
        {
            get
            {
                if (this._exitCommand == null)
                {
                    this._exitCommand = new DelegateCommand(Exit, canExecute);
                }

                return this._exitCommand;
            }
        }

        private void Exit()
        {
            Application.Current.MainWindow.Close();
        }

        private bool canExecute(object _)
        {
            return true;
        }

        private bool canExecuteAsync(object _)
        {
            return !this.IsBusy;
        }
        #endregion

        #region Fields
        private const string appName = "GB Live";
        private const string upcomingBegins = "<dl class=\"promo-upcoming\">";
        private const string upcomingEnds = "</dl>";
        private DispatcherTimer _updateTimer = null;
        #endregion

        #region Properties
        private string _windowTitle = appName;
        public string WindowTitle
        {
            get { return this._windowTitle; }
            set
            {
                this._windowTitle = value;

                OnNotifyPropertyChanged();
            }
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get { return this._isLive; }
            set
            {
                if (!value == _isLive) // only need to change anything if value and the field are different
                {
                    this._isLive = value;

                    OnNotifyPropertyChanged();
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }
            set
            {
                if (!value == _isBusy) // only need to change anything if value and the field are different
                {
                    this._isBusy = value;

                    OnNotifyPropertyChanged();

                    RaiseAllCommandCanExecuteChanged();
                }
            }
        }

        private void RaiseAllCommandCanExecuteChanged()
        {
            this.RefreshCommandAsync.RaiseCanExecuteChanged();
        }

        private ObservableCollection<GBUpcomingEvent> _events = new ObservableCollection<GBUpcomingEvent>();
        public ObservableCollection<GBUpcomingEvent> Events { get { return this._events; } }
        #endregion

        public ViewModel()
        {
            Application.Current.MainWindow.Loaded += MainWindow_Loaded;

            this._updateTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 4, 0), // 0 hours, 4 minutes, 0 seconds
                IsEnabled = false
            };

            this._updateTimer.Tick += _updateTimer_Tick;
            this._updateTimer.IsEnabled = true;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        private async void _updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync()
        {
            this.WindowTitle = string.Format("{0}: checking ...", appName);
            this.IsBusy = true;

            await Task.WhenAll(IsGiantBombLiveAsync(), UpdateUpcomingEventsAsync()).ConfigureAwait(false);

            this.IsBusy = false;
            this.WindowTitle = appName;
        }

        private async Task IsGiantBombLiveAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Globals.gbChat);
            string websiteAsString = await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);

            if (String.IsNullOrEmpty(websiteAsString) == false)
            {
                if (websiteAsString.Contains("There is currently no show"))
                {
                    this.IsLive = false;
                }
                else
                {
                    if (this.IsLive == false)
                    {
                        Utils.SafeDispatcher(() =>
                            {
                                NotificationService.Send("Giantbomb is LIVE", Globals.gbChat);
                            },
                            DispatcherPriority.Background);

                        this.IsLive = true;
                    }
                }
            }
        }

        private async Task UpdateUpcomingEventsAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Globals.gbHome);

            // do NOT use ConfigureAwait(false)
            // or you would have to post everrything else to Dispatcher
            string websiteAsString = await Utils.DownloadWebsiteAsStringAsync(req);

            if (String.IsNullOrEmpty(websiteAsString) == false)
            {
                List<GBUpcomingEvent> eventsFromHtml = RetrieveEventsFromHtml(websiteAsString);

                this.Events.Clear();

                this.Events.AddList<GBUpcomingEvent>(eventsFromHtml);
            }

            foreach (GBUpcomingEvent each in this.Events)
            {
                each.Update();
            }
        }

        private List<GBUpcomingEvent> RetrieveEventsFromHtml(string websiteAsString)
        {
            string upcomingHtml = string.Empty;

            try
            {
                upcomingHtml = websiteAsString.FromBetween(upcomingBegins, upcomingEnds);
            }
            catch (ArgumentException)
            {
                return new List<GBUpcomingEvent>(0);
            }

            return ParseHtmlForEvents(upcomingHtml);
        }

        private List<GBUpcomingEvent> ParseHtmlForEvents(string upcomingHtml)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();

            string dd = string.Empty;
            bool shouldConcat = false;

            using (StringReader sr = new StringReader(upcomingHtml))
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

                        GBUpcomingEvent newEvent = null;

                        if (GBUpcomingEvent.TryCreate(dd, out newEvent))
                        {
                            if (newEvent.Time > DateTime.Now)
                            {
                                events.Add(newEvent);
                            }

                            dd = string.Empty;
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();

                            sb.AppendLine("An event could not be created from the following HTML:");
                            sb.AppendLine(dd);

                            Utils.LogMessage(sb.ToString());
                        }
                    }

                    if (shouldConcat)
                    {
                        dd += line;
                    }
                }
            }

            return events;
        }

        private HttpWebRequest BuildHttpWebRequest(Uri gbUri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(gbUri);

            req.Accept = "text/html";
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Host = gbUri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format("{0}://{1}/", gbUri.GetLeftPart(UriPartial.Scheme), gbUri.DnsSafeHost);
            req.Timeout = 2500;
            req.UserAgent = "IE11: Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            req.Headers.Add("DNT", "1");
            //req.Headers.Add("Accept-Language", "en-gb");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");

            // 1) lu is probably country-code, returned timezone has yet to be wrong after setting this
            // 2) after months of use, the prior claim remains true
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(gbUri, new Cookie("xcg", "lu"));

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.GetType().ToString());
            sb.AppendLine(this.WindowTitle);
            sb.AppendLine(string.Format("IsLive: {0}", this.IsLive));
            sb.AppendLine(string.Format("Number of events: {0}", this.Events.Count));

            return sb.ToString();
        }
    }
}
