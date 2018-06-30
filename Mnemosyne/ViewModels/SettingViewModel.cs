using Mnemosyne.Helpers;
using Mnemosyne.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mnemosyne.ViewModels
{
	public class SettingViewModel : BaseViewModel
	{
		public List<FontSize> FontSizes
			=> Enum.GetValues(typeof(FontSize)).Cast<FontSize>().ToList();

		public RelayAction ReturnPreviousPage { get; }

		public RelayAction RestoreDefaultsAction { get; }

		public SettingViewModel()
		{
			ReturnPreviousPage = new RelayAction((parameter) =>
			{
				((Frame)Window.Current.Content).GoBack();
			}, (parameter) =>
			{
				return true;
			});

			RestoreDefaultsAction = new RelayAction((parameter) =>
			{
				Settings.RestoreDefaults();;
			}, (parameter) =>
			{
				return true;
			});
		}
	}
}
