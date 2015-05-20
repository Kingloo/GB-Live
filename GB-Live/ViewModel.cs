﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
                if (_refreshCommandAsync == null)
                {
                    _refreshCommandAsync = new DelegateCommandAsync(RefreshAsync, canExecuteAsync);
                }

                return _refreshCommandAsync;
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
                if (_goToHomePageInBrowserCommand == null)
                {
                    _goToHomePageInBrowserCommand = new DelegateCommand(GoToHomePageInBrowser, canExecute);
                }

                return _goToHomePageInBrowserCommand;
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
                if (_goToChatPageInBrowserCommand == null)
                {
                    _goToChatPageInBrowserCommand = new DelegateCommand(GoToChatPageInBrowser, canExecute);
                }

                return _goToChatPageInBrowserCommand;
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
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(Exit, canExecute);
                }

                return _exitCommand;
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
            return !IsBusy;
        }
        #endregion

        #region Fields
        private const string appName = "GB Live";
        private const string upcomingBegins = "<dl class=\"promo-upcoming\">";
        private const string upcomingEnds = "</dl>";
        private DispatcherTimer updateTimer = null;
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
        public ObservableCollection<GBUpcomingEvent> Events { get { return _events; } }
        #endregion

        public ViewModel()
        {
            Application.Current.MainWindow.Loaded += MainWindow_Loaded;
            Events.CollectionChanged += Events_CollectionChanged;

            updateTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 4, 0), // 0 hours, 4 minutes, 0 seconds
            };

            updateTimer.Tick += updateTimer_Tick;
            updateTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR !!!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventsArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             *
             */

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    StartAllEventTimers(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    StopAllEventTimers(e.OldItems);
                    break;
                default:
                    break;
            }
        }

        private void StartAllEventTimers(IList newItems)
        {
            foreach (GBUpcomingEvent each in newItems)
            {
                each.StartCountdownTimer();
            }
        }

        private void StopAllEventTimers(IList oldItems)
        {
            foreach (GBUpcomingEvent each in oldItems)
            {
                each.StopCountdownTimer();
            }
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync()
        {
            this.WindowTitle = string.Format("{0}: checking ...", appName);
            this.IsBusy = true;

            string websiteAsString = await GetHomepage(Globals.gbHome);

            if (String.IsNullOrWhiteSpace(websiteAsString)) return;

            CheckIfGiantBombIsLive(websiteAsString);

            UpdateUpcomingEvents(websiteAsString);

            this.IsBusy = false;
            this.WindowTitle = appName;
        }

        private async Task<string> GetHomepage(Uri uri)
        {
            HttpWebRequest req = BuildHttpWebRequest(uri);

            string websiteAsString = await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);

            return websiteAsString;
        }

        private void CheckIfGiantBombIsLive(string websiteAsString)
        {
            if (websiteAsString.Contains("Live on Giant Bomb!"))
            {
                if (IsLive == false)
                {
                    IsLive = true;

                    Utils.SafeDispatcher(() =>
                        NotificationService.Send("GiantBomb is LIVE", Globals.gbChat),
                        DispatcherPriority.Background);
                }
            }
            else
            {
                IsLive = false;
            }
        }

        private void UpdateUpcomingEvents(string websiteAsString)
        {
            List<GBUpcomingEvent> eventsFromHtml = RetrieveEventsFromHtml(websiteAsString);

            if (eventsFromHtml.Count > 0)
            {
                Events.AddMissing<GBUpcomingEvent>(eventsFromHtml);
            }

            Remove(eventsFromHtml);
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

        private void Remove(List<GBUpcomingEvent> eventsFromHtml)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR !!!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventsArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             *
             */
            
            List<GBUpcomingEvent> toRemove = (from each in this.Events
                                              where each.Time.Equals(DateTime.MaxValue) || eventsFromHtml.Contains(each) == false
                                              select each)
                                              .ToList<GBUpcomingEvent>();

            foreach (GBUpcomingEvent each in toRemove)
            {
                this.Events.Remove(each);
            }
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
