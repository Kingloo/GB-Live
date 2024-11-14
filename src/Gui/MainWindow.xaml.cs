using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace GBLive.Gui
{
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewModel viewModel;

		public MainWindow(MainWindowViewModel viewModel)
		{
			InitializeComponent();

			Loaded += Window_Loaded;
			KeyDown += Window_KeyDown;
			Closing += Window_Closing;

			Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

			this.viewModel = viewModel;

			DataContext = viewModel;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			await viewModel.UpdateAsync().ConfigureAwait(true);

			viewModel.StartTimer();
		}

		private async void Window_KeyDown(object sender, KeyEventArgs e)
		{
#pragma warning disable IDE0010 // complains that you haven't specified every enum case
			switch (e.Key)
			{
				case Key.Escape:
					Close();
					break;
				case Key.F5:
					{
						await viewModel.UpdateAsync().ConfigureAwait(true);
						break;
					}
				default:
					break;
			}
#pragma warning restore IDE0010
		}

		private void ItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				viewModel.OpenHomePage();
			}
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				viewModel.OpenChatPage();
			}
		}

		private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			viewModel.StopTimer();

			Loaded -= Window_Loaded;
			KeyDown -= Window_KeyDown;
			Closing -= Window_Closing;
		}
	}
}
