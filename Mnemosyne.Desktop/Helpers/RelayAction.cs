using System;
using System.Windows.Input;

namespace Mnemosyne.Desktop.Helpers
{
	public class RelayAction : ICommand
	{
		private Action<object> method;
		private Predicate<object> predicate;

		public event EventHandler CanExecuteChanged;

		public RelayAction(Action<object> method, Predicate<object> predicate)
		{
			this.method = method ?? throw new ArgumentNullException(nameof(method));
			this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public void Notify()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool CanExecute(object parameter)
		{
			return predicate(parameter);
		}

		public void Execute(object parameter)
		{
			method(parameter);
		}
	}
}
