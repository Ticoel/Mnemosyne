using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Mnemosyne.Desktop.ViewModels;

namespace Mnemosyne.Desktop.Views
{
	/// <summary>
	/// Logique d'interaction pour ProfilWindow.xaml
	/// </summary>
	public partial class VisualizationView : Window
    {
		private VisualizationViewModel VisualizationViewModel;

        public VisualizationView(ProfileViewModel profil)
        {
            InitializeComponent();

			VisualizationViewModel = ((VisualizationViewModel)DataContext);

			VisualizationViewModel.Profil = profil;

			Closed += (sender, e) => { if (VisualizationViewModel.Profil.IsModifiable) VisualizationViewModel.CMDSave.Execute(false); };
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			VisualizationViewModel.Remove();
			Close();
		}
	}
}
