using System;
using System.Windows.Input;

namespace Mnemosyne.Helpers
{
	public class RelayAction : ICommand
	{
		public Action<object> Method { get; }
		public Predicate<object> Predicate { get; set; }

		public event EventHandler CanExecuteChanged;

		public RelayAction(Action<object> method, Predicate<object> predicate)
		{
			Method = method ?? throw new ArgumentNullException(nameof(method));
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public void NotifyCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool CanExecute(object parameter)
		{
			return Predicate(parameter);
		}

		public void Execute(object parameter)
		{
			Method(parameter);
		}
	}
}
