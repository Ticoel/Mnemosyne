using Mnemosyne.Desktop.Helpers;
using Mnemosyne.Desktop.Properties;
using System.ComponentModel;
using System.Windows.Controls;

namespace Mnemosyne.ViewModels
{
	public abstract class CommonViewModel : INotifyPropertyChanged
	{
		public FontSize FontSize
		{
			get => (FontSize)Settings.Default.FontSize;
			set
			{
				if ((int)value != Settings.Default.FontSize)
				{
					Settings.Default.FontSize = (int)value;
					Settings.Default.Save();
					Notify(nameof(FontSize));
				}
			}
		}

		public bool UpdateCreationTime
		{
			get => Settings.Default.UpdateCreationTime;
			set
			{
				if (value != Settings.Default.UpdateCreationTime)
				{
					Settings.Default.UpdateCreationTime = value;
					Settings.Default.Save();
					Notify(nameof(UpdateCreationTime));
				}
			}
		}

		public bool UpdateLastAccessTime
		{
			get => Settings.Default.UpdateLastAccessTime;
			set
			{
				if (value != Settings.Default.UpdateLastAccessTime)
				{
					Settings.Default.UpdateLastAccessTime = value;
					Settings.Default.Save();
					Notify(nameof(UpdateLastAccessTime));
				}
			}
		}

		public bool UpdateLastWriteTime
		{
			get => Settings.Default.UpdateLastAccessTime;
			set
			{
				if (value != Settings.Default.UpdateLastWriteTime)
				{
					Settings.Default.UpdateLastWriteTime = value;
					Settings.Default.Save();
					Notify(nameof(UpdateLastWriteTime));
				}
			}
		}

		public bool UpdateAttributes
		{
			get => Settings.Default.UpdateAttributes;
			set
			{
				if (value != Settings.Default.UpdateAttributes)
				{
					Settings.Default.UpdateAttributes = value;
					Settings.Default.Save();
					Notify(nameof(UpdateAttributes));
				}
			}
		}

		public bool UpdateAccessControl
		{
			get => Settings.Default.UpdateAccessControl;
			set
			{
				if (value != Settings.Default.UpdateAccessControl)
				{
					Settings.Default.UpdateAccessControl = value;
					Settings.Default.Save();
					Notify(nameof(UpdateAccessControl));
				}
			}
		}

		public int BufferLength
		{
			get => Settings.Default.BufferLength;
			set
			{
				if (value != Settings.Default.BufferLength)
				{
					Settings.Default.BufferLength = value;
					Settings.Default.Save();
					Notify(nameof(BufferLength));
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
