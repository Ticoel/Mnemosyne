using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mnemosyne.Desktop.Views
{
    public partial class CommonView
    {
		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			((Popup)((ListView)sender).FindName("Popup")).IsOpen = false;
		}

		private void ListView_GotFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
		}

		private void ToggleButton_GotFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = false;
		}
	}
}
