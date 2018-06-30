using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
	{
		public FontSize FontSize
		{
			get
			{
				Settings.AddListener(GetType(), (sender, e) => { Notify(nameof(FontSize)); });
				return Settings.FontSize;
			}
			set
			{
				if (value != Settings.FontSize)
				{
					Settings.FontSize = value;
					Notify("FontSize");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Notify(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
