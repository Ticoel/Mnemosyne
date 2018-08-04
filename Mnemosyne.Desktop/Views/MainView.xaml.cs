using Mnemosyne.Desktop.Models;
using Mnemosyne.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mnemosyne.Desktop.Views
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			
			((Popup)((ListView)sender).FindName("Popup")).IsOpen = false;
		}

		private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).GetProfils();
		}

		private void ListView_GotFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
		}

		private void ToggleButton_GotFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = false;
		}

		private void Window_GotFocus(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).GetProfils();
		}
	}
}
