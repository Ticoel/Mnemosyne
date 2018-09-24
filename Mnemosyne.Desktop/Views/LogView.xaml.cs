using Mnemosyne.Desktop.ViewModels;
using System.Windows;

namespace Mnemosyne.Desktop.Views
{
	public partial class LogView : Window
	{
		LogViewModel logViewModel;

		public LogView()
		{
			InitializeComponent();

			logViewModel = (LogViewModel)DataContext;
		}

		private void ListView_Loaded(object sender, RoutedEventArgs e)
		{
			logViewModel.CMDGetLogFiles.Execute(null);
		}

		private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			logViewModel.CMDOpenLogFile.Execute(null);
		}
	}
}
