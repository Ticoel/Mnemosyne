using Mnemosyne.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mnemosyne.Desktop.ViewModels;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mnemosyne.Desktop.Views
{
    /// <summary>
    /// Logique d'interaction pour ProfilWindow.xaml
    /// </summary>
    public partial class ProfilWindow : Window
    {
        public ProfilWindow(Profile profil)
        {
            InitializeComponent();

			((ProfilViewModel)DataContext).Profil = profil;

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			((ProfilViewModel)DataContext).Remove();
			Close();
		}
	}
}
