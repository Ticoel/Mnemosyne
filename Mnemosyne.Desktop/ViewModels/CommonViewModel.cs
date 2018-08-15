using Mnemosyne.Desktop.Helpers;
using Mnemosyne.Desktop.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace Mnemosyne.Desktop.ViewModels
{
	public abstract class CommonViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void Notify(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
