using Mnemosyne.Desktop.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mnemosyne.Desktop.Views
{
	public partial class AddingView : Window
	{
		ProfileAddingViewModel addingViewModel;

		public AddingView(string sourcePath)
		{
			InitializeComponent();

			addingViewModel = (ProfileAddingViewModel)DataContext;

			addingViewModel.SourcePath = sourcePath;
		}

		private void Button_Click_Close(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TextBox_TextChanged_CheckDigitInput(object sender, TextChangedEventArgs e)
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

		private void TextBox_TextChanged_CheckCMDCreateProfile(object sender, TextChangedEventArgs e)
		{
			addingViewModel.CMDCreateProfile.Notify();
		}
	}
}
