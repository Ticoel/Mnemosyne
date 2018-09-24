using Mnemosyne.Desktop.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mnemosyne.Desktop.Views
{
	public partial class ProfileVisualizationView : Window
    {
		private ProfileVisualizationViewModel visualizationViewModel;

        public ProfileVisualizationView(string sourcePath, ProfileViewModel profil)
        {
            InitializeComponent();

			visualizationViewModel = ((ProfileVisualizationViewModel)DataContext);

			visualizationViewModel.SourcePath = sourcePath;
			visualizationViewModel.Profile = profil;
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

		private void Button_Click_RemoveProfile(object sender, RoutedEventArgs e)
		{
			visualizationViewModel.CMDRemoveProfile.Execute(null);
			Close();
		}
	}
}
