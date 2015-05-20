using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GB_Live
{
    public static class NotificationService
    {
        public static void Send(string send)
        {
            Notification n = new Notification(send);
            n.Send();
        }

        public static void Send(string send, string description)
        {
            Notification n = new Notification(send, description);
            n.Send();
        }

        public static void Send(string send, Uri uri)
        {
            Notification n = new Notification(send, uri);
            n.Send();
        }

        public static void Send(string send, string description, Uri uri)
        {
            Notification n = new Notification(send, description, uri);
            n.Send();
        }

        private class Notification
        {
            #region Properties
            private string _title = string.Empty;
            public string Title
            {
                get
                {
                    return this._title;
                }
            }

            private string _description = string.Empty;
            public string Description
            {
                get
                {
                    return this._description;
                }
            }

            private Uri _uri = null;
            public Uri Uri
            {
                get
                {
                    return this._uri;
                }
            }
            #endregion

            public Notification(string title)
            {
                this._title = title;
            }

            public Notification(string title, string description)
            {
                this._title = title;
                this._description = description;
            }

            public Notification(string title, Uri doubleClickUri)
            {
                this._title = title;
                this._uri = doubleClickUri;
            }

            public Notification(string title, string description, Uri doubleClickUri)
            {
                this._title = title;
                this._description = description;
                this._uri = doubleClickUri;
            }

            public void Send()
            {
                NotificationWindow notificationWindow = new NotificationWindow(this);
            }
        }

        private class NotificationWindow : Window
        {
            private Notification n = null;

            public NotificationWindow(Notification n)
            {
                this.n = n;

                this.Owner = Application.Current.MainWindow;
                this.Style = BuildWindowStyle();

                Grid grid = new Grid
                {
                    Style = BuildGridStyle()
                };

                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });

                Label lbl_Title = new Label
                {
                    Style = BuildLabelTitleStyle(),
                    Content = new TextBlock
                    {
                        Text = n.Title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };

                Grid.SetRow(lbl_Title, 0);
                grid.Children.Add(lbl_Title);

                if (String.IsNullOrEmpty(n.Description) == false)
                {
                    Label lbl_Description = new Label
                    {
                        Style = BuildLabelDescriptionStyle(),
                        Content = new TextBlock
                        {
                            Text = n.Description,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            FontStyle = FontStyles.Italic
                        }
                    };

                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });

                    Grid.SetRow(lbl_Description, 1);
                    grid.Children.Add(lbl_Description);
                }

                this.AddChild(grid);

                CountdownDispatcherTimer expirationTimer = new CountdownDispatcherTimer(new TimeSpan(0, 0, 15), () => this.Close());

                DisplayThisWindow();
            }

            private Style BuildWindowStyle()
            {
                Style style = new Style(typeof(NotificationWindow));

                style.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler((sender, e) =>
                {
                    // we deliberately avoid using Utils.OpenUriInBrowser to avoid the dependency

                    Process.Start(this.n.Uri.AbsoluteUri);
                })));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.Transparent));

                style.Setters.Add(new Setter(TopmostProperty, true));
                style.Setters.Add(new Setter(FocusableProperty, false));
                style.Setters.Add(new Setter(ShowInTaskbarProperty, false));
                style.Setters.Add(new Setter(ShowActivatedProperty, false));
                style.Setters.Add(new Setter(IsTabStopProperty, false));
                style.Setters.Add(new Setter(ResizeModeProperty, ResizeMode.NoResize));
                style.Setters.Add(new Setter(WindowStyleProperty, WindowStyle.None));
                style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0d)));

                double top = SystemParameters.WorkArea.Top + 50;
                double left = SystemParameters.WorkArea.Right - 475d - 100;

                style.Setters.Add(new Setter(TopProperty, top));
                style.Setters.Add(new Setter(LeftProperty, left));

                style.Setters.Add(new Setter(SizeToContentProperty, SizeToContent.Height));
                style.Setters.Add(new Setter(WidthProperty, 475d));

                return style;
            }

            private Style BuildGridStyle()
            {
                Style style = new Style(typeof(Grid));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));

                style.Setters.Add(new Setter(HeightProperty, Double.NaN));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

                style.Setters.Add(new Setter(WidthProperty, Double.NaN));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

                return style;
            }

            private Style BuildLabelTitleStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(15, 0, 15, 0)));
                style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Calibri")));
                style.Setters.Add(new Setter(FontSizeProperty, 22d));
                style.Setters.Add(new Setter(HeightProperty, 75d));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

                return style;
            }

            private Style BuildLabelDescriptionStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 15, 0)));
                style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Calibri")));
                style.Setters.Add(new Setter(FontSizeProperty, 14d));
                style.Setters.Add(new Setter(HeightProperty, 40d));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Right));

                return style;
            }

            private void DisplayThisWindow()
            {
                this.Show();

                System.Media.SystemSounds.Hand.Play();
            }
        }
    }
}
