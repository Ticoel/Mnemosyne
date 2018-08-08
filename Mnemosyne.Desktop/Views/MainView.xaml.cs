using Mnemosyne.Desktop.ViewModels;
using System.Windows;

namespace Mnemosyne.Desktop.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ComboBox_GetProfils(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).GetProfils();
		}
	}
}
