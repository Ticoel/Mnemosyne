using System.ComponentModel;
using System.Diagnostics;

namespace Mnemosyne.Desktop.ViewModels
{
	public abstract class CommonViewModel : INotifyPropertyChanged
	{
		public enum MessageType
		{
			Information,
			Warning,
			Error
		}

		public string Output
		{
			get => output;
			set
			{
				if (value != output)
				{
					output = value;
					Notify(nameof(Output));
				}
			}
		}

		private string output;

		public event PropertyChangedEventHandler PropertyChanged;

		public void Notify(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void WriteMessage(string message, MessageType type, bool canFlush = true)
		{
			switch (type)
			{
				case MessageType.Information:
					Trace.TraceInformation(message);
					break;
				case MessageType.Warning:
					Trace.TraceWarning(message);
					break;
				case MessageType.Error:
					Trace.TraceError(message);
					break;
				default:
					break;
			}

			if (canFlush)
				Trace.Flush();

			Output = message;
		}
	}
}
