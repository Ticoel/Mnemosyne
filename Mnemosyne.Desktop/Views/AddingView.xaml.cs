using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mnemosyne.Desktop.Views
{
	/// <summary>
	/// Logique d'interaction pour ProfileAddingWindow.xaml
	/// </summary>
	public partial class ProfileAddingWindow : Window
	{
		public ProfileAddingWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var texBox = (TextBox)sender;

			if (!int.TryParse(texBox.Text, out int result))
			{
				var offset = e.Changes.First().Offset;
				var length = e.Changes.First().AddedLength;
				texBox.Text = texBox.Text.Remove(offset, length);
				texBox.CaretIndex = offset;
			}
		}
	}
}
